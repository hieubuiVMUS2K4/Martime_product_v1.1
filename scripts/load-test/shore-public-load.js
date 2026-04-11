import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend } from 'k6/metrics';

const BASE_URL = (__ENV.BASE_URL || 'https://shcdvmu.site').replace(/\/$/, '');
const ACCESS_TOKEN = (__ENV.ACCESS_TOKEN || '').trim();
const TARGET_VUS = Number(__ENV.TARGET_VUS || 100);
const RAMP_UP = __ENV.RAMP_UP || '2m';
const HOLD = __ENV.HOLD || '10m';
const RAMP_DOWN = __ENV.RAMP_DOWN || '1m';
const MIN_THINK_MS = Number(__ENV.MIN_THINK_MS || 800);
const MAX_THINK_MS = Number(__ENV.MAX_THINK_MS || 2500);
const VOYAGES_PAGE_SIZE = Number(__ENV.VOYAGES_PAGE_SIZE || 10);
const REPORTS_PAGE_SIZE = Number(__ENV.REPORTS_PAGE_SIZE || 10);

const endpointDuration = new Trend('shore_endpoint_duration', true);

function buildUrl(path) {
  return `${BASE_URL}${path}`;
}

function buildParams(endpoint) {
  const headers = {
    Accept: 'application/json',
    'User-Agent': `k6-shore-public-load/${TARGET_VUS}vu`,
    'Cache-Control': 'no-cache',
  };

  if (ACCESS_TOKEN) {
    headers.Authorization = `Bearer ${ACCESS_TOKEN}`;
  }

  return {
    headers,
    tags: {
      endpoint,
      auth_mode: ACCESS_TOKEN ? 'bearer' : 'public',
    },
    timeout: '30s',
  };
}

function recordChecks(response, endpoint) {
  endpointDuration.add(response.timings.duration, { endpoint });
  return check(response, {
    [`${endpoint} status is 200`]: (res) => res.status === 200,
    [`${endpoint} body is not empty`]: (res) => Boolean(res.body && res.body.length > 0),
  });
}

function getJson(path, endpoint) {
  const response = http.get(buildUrl(path), buildParams(endpoint));
  recordChecks(response, endpoint);

  let payload = null;
  try {
    payload = response.json();
  } catch (error) {
    check(null, {
      [`${endpoint} returns valid json`]: () => false,
    });
  }

  return { response, payload };
}

function randomInt(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

function randomItem(items) {
  if (!Array.isArray(items) || items.length === 0) {
    return null;
  }

  return items[randomInt(0, items.length - 1)];
}

function pauseLikeUser() {
  const thinkMs = randomInt(MIN_THINK_MS, MAX_THINK_MS);
  sleep(thinkMs / 1000);
}

function chooseVoyagesPage(total) {
  const safeTotal = Number(total || 0);
  if (safeTotal <= VOYAGES_PAGE_SIZE) {
    return 1;
  }

  const totalPages = Math.ceil(safeTotal / VOYAGES_PAGE_SIZE);
  return randomInt(1, totalPages);
}

export const options = {
  scenarios: {
    shore_public_read_only: {
      executor: 'ramping-vus',
      startVUs: 0,
      gracefulRampDown: '30s',
      stages: [
        { duration: RAMP_UP, target: TARGET_VUS },
        { duration: HOLD, target: TARGET_VUS },
        { duration: RAMP_DOWN, target: 0 },
      ],
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.02'],
    http_req_duration: ['p(95)<2000', 'p(99)<5000'],
    checks: ['rate>0.98'],
    'http_req_duration{endpoint:health}': ['p(95)<500'],
    'http_req_duration{endpoint:vessels}': ['p(95)<2500'],
    'http_req_duration{endpoint:fleet-summary}': ['p(95)<2500'],
    'http_req_duration{endpoint:voyages-list}': ['p(95)<2500'],
    'http_req_duration{endpoint:voyage-detail}': ['p(95)<3000'],
    'http_req_duration{endpoint:reports-vessels}': ['p(95)<2500'],
    'http_req_duration{endpoint:reports-vessel-detail}': ['p(95)<3000'],
  },
  summaryTrendStats: ['avg', 'min', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
  userAgent: `k6-shore-public-load/${TARGET_VUS}vu`,
};

export function setup() {
  const health = getJson('/api/health', 'health');
  check(health.payload, {
    'health payload status is healthy': (payload) => payload && payload.status === 'healthy',
  });

  const vessels = getJson('/api/vessels', 'vessels');
  const reportsVessels = getJson('/api/reports/vessels', 'reports-vessels');
  const voyages = getJson(`/api/voyages?page=1&pageSize=${VOYAGES_PAGE_SIZE}`, 'voyages-list');
  const fleetSummary = getJson('/api/vessels/fleet-summary', 'fleet-summary');

  const vesselItems = Array.isArray(vessels.payload) ? vessels.payload : [];
  const reportVesselItems = Array.isArray(reportsVessels.payload) ? reportsVessels.payload : [];
  const voyageItems = Array.isArray(voyages.payload?.data) ? voyages.payload.data : [];
  const fleetItems = Array.isArray(fleetSummary.payload) ? fleetSummary.payload : [];

  return {
    vesselIds: vesselItems.map((item) => item.id).filter(Boolean),
    reportVesselIds: reportVesselItems.map((item) => item.id).filter(Boolean),
    voyageIds: voyageItems.map((item) => item.id).filter(Boolean),
    voyagesTotal: Number(voyages.payload?.total || voyageItems.length || 0),
    fleetCount: fleetItems.length,
  };
}

export default function (data) {
  const roll = Math.random() * 100;

  if (roll < 5) {
    const health = getJson('/api/health', 'health');
    check(health.payload, {
      'health stays healthy during load': (payload) => payload && payload.status === 'healthy',
    });
  } else if (roll < 30) {
    getJson('/api/vessels', 'vessels');
  } else if (roll < 50) {
    getJson('/api/vessels/fleet-summary', 'fleet-summary');
  } else if (roll < 75) {
    const page = chooseVoyagesPage(data.voyagesTotal);
    getJson(`/api/voyages?page=${page}&pageSize=${VOYAGES_PAGE_SIZE}`, 'voyages-list');
  } else if (roll < 85 && data.voyageIds.length > 0) {
    const voyageId = randomItem(data.voyageIds);
    getJson(`/api/voyages/${voyageId}`, 'voyage-detail');
  } else if (roll < 95) {
    getJson('/api/reports/vessels', 'reports-vessels');
  } else if (data.reportVesselIds.length > 0) {
    const vesselId = randomItem(data.reportVesselIds);
    getJson(`/api/reports/vessel/${vesselId}?page=1&pageSize=${REPORTS_PAGE_SIZE}`, 'reports-vessel-detail');
  } else if (data.vesselIds.length > 0) {
    const vesselId = randomItem(data.vesselIds);
    getJson(`/api/vessels/${vesselId}`, 'vessel-detail-fallback');
  } else {
    getJson('/api/health', 'health');
  }

  pauseLikeUser();
}