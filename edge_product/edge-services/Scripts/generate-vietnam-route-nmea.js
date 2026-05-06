#!/usr/bin/env node

"use strict";

const fs = require("fs");
const path = require("path");

const DEFAULT_WAYPOINTS = [
  { name: "VungTau_Offshore", lat: 10.15, lon: 107.45 },
  { name: "PhuQuy_Sea", lat: 10.95, lon: 108.85 },
  { name: "CamRanh_Offshore", lat: 11.85, lon: 109.95 },
  { name: "DaNang_Offshore", lat: 15.95, lon: 109.55 },
  { name: "QuangTri_Sea", lat: 17.35, lon: 108.55 },
  { name: "HaiPhong_Gulf", lat: 20.55, lon: 107.20 }
];

function parseArgs(argv) {
  const args = {};
  for (let i = 0; i < argv.length; i += 1) {
    const token = argv[i];
    if (!token.startsWith("--")) continue;

    const key = token.slice(2);
    const next = argv[i + 1];

    if (!next || next.startsWith("--")) {
      args[key] = true;
      continue;
    }

    args[key] = next;
    i += 1;
  }
  return args;
}

function toRadians(deg) {
  return (deg * Math.PI) / 180;
}

function toDegrees(rad) {
  return (rad * 180) / Math.PI;
}

function normalize360(deg) {
  const result = deg % 360;
  return result < 0 ? result + 360 : result;
}

function haversineDistanceNm(a, b) {
  const earthRadiusNm = 3440.065;

  const lat1 = toRadians(a.lat);
  const lat2 = toRadians(b.lat);
  const dLat = toRadians(b.lat - a.lat);
  const dLon = toRadians(b.lon - a.lon);

  const sinLat = Math.sin(dLat / 2);
  const sinLon = Math.sin(dLon / 2);

  const h = sinLat * sinLat + Math.cos(lat1) * Math.cos(lat2) * sinLon * sinLon;
  const c = 2 * Math.atan2(Math.sqrt(h), Math.sqrt(1 - h));

  return earthRadiusNm * c;
}

function initialBearingDeg(a, b) {
  const lat1 = toRadians(a.lat);
  const lat2 = toRadians(b.lat);
  const dLon = toRadians(b.lon - a.lon);

  const y = Math.sin(dLon) * Math.cos(lat2);
  const x =
    Math.cos(lat1) * Math.sin(lat2) -
    Math.sin(lat1) * Math.cos(lat2) * Math.cos(dLon);

  return normalize360(toDegrees(Math.atan2(y, x)));
}

function buildSegments(waypoints) {
  const segments = [];
  for (let i = 0; i < waypoints.length - 1; i += 1) {
    const start = waypoints[i];
    const end = waypoints[i + 1];
    const distanceNm = haversineDistanceNm(start, end);
    const bearingDeg = initialBearingDeg(start, end);

    segments.push({
      start,
      end,
      distanceNm,
      bearingDeg
    });
  }
  return segments;
}

function interpolatePosition(start, end, fraction) {
  return {
    lat: start.lat + (end.lat - start.lat) * fraction,
    lon: start.lon + (end.lon - start.lon) * fraction
  };
}

function locatePositionAlongRoute(segments, totalDistanceNm, traveledNm, loopRoute) {
  let remaining = traveledNm;

  if (loopRoute && totalDistanceNm > 0) {
    remaining %= totalDistanceNm;
  } else {
    remaining = Math.min(remaining, totalDistanceNm);
  }

  for (const segment of segments) {
    if (segment.distanceNm <= 0) {
      continue;
    }

    if (remaining <= segment.distanceNm) {
      const fraction = remaining / segment.distanceNm;
      const position = interpolatePosition(segment.start, segment.end, fraction);
      return {
        lat: position.lat,
        lon: position.lon,
        courseDeg: segment.bearingDeg
      };
    }

    remaining -= segment.distanceNm;
  }

  const last = segments[segments.length - 1];
  return {
    lat: last.end.lat,
    lon: last.end.lon,
    courseDeg: last.bearingDeg
  };
}

function checksum(sentenceBody) {
  let xor = 0;
  for (let i = 0; i < sentenceBody.length; i += 1) {
    xor ^= sentenceBody.charCodeAt(i);
  }
  return xor.toString(16).toUpperCase().padStart(2, "0");
}

function wrapSentence(sentenceBody) {
  return `$${sentenceBody}*${checksum(sentenceBody)}`;
}

function formatUtcTime(date) {
  const hh = String(date.getUTCHours()).padStart(2, "0");
  const mm = String(date.getUTCMinutes()).padStart(2, "0");
  const ss = String(date.getUTCSeconds()).padStart(2, "0");
  const ms = String(date.getUTCMilliseconds()).padStart(3, "0");
  return `${hh}${mm}${ss}.${ms}`;
}

function formatUtcDate(date) {
  const dd = String(date.getUTCDate()).padStart(2, "0");
  const mm = String(date.getUTCMonth() + 1).padStart(2, "0");
  const yy = String(date.getUTCFullYear()).slice(-2);
  return `${dd}${mm}${yy}`;
}

function toNmeaLatitude(lat) {
  const hemisphere = lat >= 0 ? "N" : "S";
  const absolute = Math.abs(lat);
  const degrees = Math.floor(absolute);
  const minutes = (absolute - degrees) * 60;

  const degPart = String(degrees).padStart(2, "0");
  const minPart = minutes.toFixed(3).padStart(6, "0");

  return {
    value: `${degPart}${minPart}`,
    hemisphere
  };
}

function toNmeaLongitude(lon) {
  const hemisphere = lon >= 0 ? "E" : "W";
  const absolute = Math.abs(lon);
  const degrees = Math.floor(absolute);
  const minutes = (absolute - degrees) * 60;

  const degPart = String(degrees).padStart(3, "0");
  const minPart = minutes.toFixed(3).padStart(6, "0");

  return {
    value: `${degPart}${minPart}`,
    hemisphere
  };
}

function buildGga(date, lat, lon, fixQuality, satellitesUsed, hdop, altitudeMeters) {
  const t = formatUtcTime(date);
  const la = toNmeaLatitude(lat);
  const lo = toNmeaLongitude(lon);

  const body = [
    "GPGGA",
    t,
    la.value,
    la.hemisphere,
    lo.value,
    lo.hemisphere,
    String(fixQuality),
    String(satellitesUsed).padStart(2, "0"),
    hdop.toFixed(1),
    altitudeMeters.toFixed(1),
    "M",
    "0.0",
    "M",
    "",
    ""
  ].join(",");

  return wrapSentence(body);
}

function buildGsa(satellitesUsed, pdop, hdop, vdop) {
  const satFields = [];
  for (let i = 1; i <= 12; i += 1) {
    satFields.push(i <= satellitesUsed ? String(i).padStart(2, "0") : "");
  }

  const body = [
    "GPGSA",
    "A",
    "3",
    ...satFields,
    pdop.toFixed(1),
    hdop.toFixed(1),
    vdop.toFixed(1)
  ].join(",");

  return wrapSentence(body);
}

function buildRmc(date, lat, lon, speedKnots, courseDeg) {
  const t = formatUtcTime(date);
  const d = formatUtcDate(date);
  const la = toNmeaLatitude(lat);
  const lo = toNmeaLongitude(lon);

  const body = [
    "GPRMC",
    t,
    "A",
    la.value,
    la.hemisphere,
    lo.value,
    lo.hemisphere,
    speedKnots.toFixed(1),
    normalize360(courseDeg).toFixed(1),
    d,
    "000.0",
    "W"
  ].join(",");

  return wrapSentence(body);
}

function toNumber(name, rawValue, fallback) {
  if (rawValue === undefined || rawValue === null || rawValue === "") {
    return fallback;
  }

  const value = Number(rawValue);
  if (!Number.isFinite(value)) {
    throw new Error(`Invalid numeric value for --${name}: ${rawValue}`);
  }
  return value;
}

function main() {
  const args = parseArgs(process.argv.slice(2));

  if (args.help) {
    // eslint-disable-next-line no-console
    console.log(
      [
        "Generate NMEA 0183 test data for a Vietnam sea route.",
        "",
        "Usage:",
        "  node ./Scripts/generate-vietnam-route-nmea.js [options]",
        "",
        "Options:",
        "  --output <path>      Output .nmea file path",
        "  --minutes <number>   Total duration in minutes (default: 180)",
        "  --interval <number>  Interval in seconds between position samples (default: 1)",
        "  --speed <number>     Speed over ground in knots (default: 14)",
        "  --start <iso-date>   UTC start datetime (default: 2026-04-26T07:45:33.144Z)",
        "  --loop <true|false>  Loop route after reaching last waypoint (default: false)",
        "",
        "Example:",
        "  npm run generate:nmea:vietnam"
      ].join("\n")
    );
    return;
  }

  const output = args.output || "./TestData/nmea/vietnam-coastal-route.nmea";
  const outputPath = path.resolve(process.cwd(), output);

  const minutes = toNumber("minutes", args.minutes, 180);
  const intervalSeconds = toNumber("interval", args.interval, 1);
  const speedKnots = toNumber("speed", args.speed, 14);

  const fixQuality = toNumber("fixQuality", args.fixQuality, 1);
  const satellitesUsed = Math.max(1, Math.min(12, Math.floor(toNumber("satellites", args.satellites, 12))));
  const hdop = toNumber("hdop", args.hdop, 1.0);
  const pdop = toNumber("pdop", args.pdop, 1.0);
  const vdop = toNumber("vdop", args.vdop, 1.0);

  if (minutes <= 0) throw new Error("--minutes must be > 0");
  if (intervalSeconds <= 0) throw new Error("--interval must be > 0");
  if (speedKnots <= 0) throw new Error("--speed must be > 0");

  const loopRoute = String(args.loop || "false").toLowerCase() === "true";

  const defaultStart = new Date(Date.UTC(2026, 3, 26, 7, 45, 33, 144));
  const startDate = args.start ? new Date(args.start) : defaultStart;
  if (Number.isNaN(startDate.getTime())) {
    throw new Error(`Invalid --start datetime: ${args.start}`);
  }

  const segments = buildSegments(DEFAULT_WAYPOINTS);
  if (segments.length === 0) {
    throw new Error("No route segments available");
  }

  const totalRouteDistanceNm = segments.reduce((sum, seg) => sum + seg.distanceNm, 0);
  const totalSeconds = Math.floor(minutes * 60);
  const sampleCount = Math.floor(totalSeconds / intervalSeconds) + 1;
  const distancePerStepNm = (speedKnots * intervalSeconds) / 3600;

  const lines = [];
  const gsaSentence = buildGsa(satellitesUsed, pdop, hdop, vdop);

  for (let i = 0; i < sampleCount; i += 1) {
    const timestamp = new Date(startDate.getTime() + i * intervalSeconds * 1000);
    const traveledNm = i * distancePerStepNm;

    const point = locatePositionAlongRoute(
      segments,
      totalRouteDistanceNm,
      traveledNm,
      loopRoute
    );

    lines.push(buildGga(timestamp, point.lat, point.lon, fixQuality, satellitesUsed, hdop, 0.0));
    lines.push(gsaSentence);
    lines.push(buildRmc(timestamp, point.lat, point.lon, speedKnots, point.courseDeg));
  }

  fs.mkdirSync(path.dirname(outputPath), { recursive: true });
  fs.writeFileSync(outputPath, `${lines.join("\n")}\n`, "utf8");

  const endTimestamp = new Date(startDate.getTime() + (sampleCount - 1) * intervalSeconds * 1000);

  // eslint-disable-next-line no-console
  console.log("NMEA file generated successfully");
  // eslint-disable-next-line no-console
  console.log(`Output: ${outputPath}`);
  // eslint-disable-next-line no-console
  console.log(`Waypoints: ${DEFAULT_WAYPOINTS.length}`);
  // eslint-disable-next-line no-console
  console.log(`Route distance: ${totalRouteDistanceNm.toFixed(2)} NM`);
  // eslint-disable-next-line no-console
  console.log(`Samples: ${sampleCount} (each sample = GGA + GSA + RMC)`);
  // eslint-disable-next-line no-console
  console.log(`Time range: ${startDate.toISOString()} -> ${endTimestamp.toISOString()}`);
}

try {
  main();
} catch (error) {
  // eslint-disable-next-line no-console
  console.error(error.message);
  process.exit(1);
}
