import React, { useState, useEffect } from 'react';
import { Card, CardHeader, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { AlertTriangle, Ship, MapPin, Fuel, Clock } from 'lucide-react';

interface Vessel {
  id: string;
  imo: string;
  name: string;
  callSign: string;
  vesselType: string;
  flag: string;
  isActive: boolean;
  lastPosition?: {
    latitude: number;
    longitude: number;
    speed?: number;
    timestamp: string;
  };
  unacknowledgedAlerts: number;
}

interface VesselAlert {
  id: string;
  vesselId: string;
  alertType: string;
  message: string;
  severity: string;
  timestamp: string;
  isAcknowledged: boolean;
  vesselName?: string;
  vesselIMO?: string;
}

const MaritimeFleetDashboard: React.FC = () => {
  const [vessels, setVessels] = useState<Vessel[]>([]);
  const [alerts, setAlerts] = useState<VesselAlert[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedSeverity, setSelectedSeverity] = useState<string>('');

  useEffect(() => {
    fetchVessels();
    fetchAlerts();
  }, []);

  const fetchVessels = async () => {
    try {
      const response = await fetch('/api/vessels');
      const data = await response.json();
      setVessels(data);
    } catch (error) {
      console.error('Error fetching vessels:', error);
    }
  };

  const fetchAlerts = async () => {
    try {
      const response = await fetch('/api/vessels/alerts/all?acknowledged=false');
      const data = await response.json();
      setAlerts(data);
      setLoading(false);
    } catch (error) {
      console.error('Error fetching alerts:', error);
      setLoading(false);
    }
  };

  const acknowledgeAlert = async (alertId: string) => {
    try {
      await fetch(`/api/vessels/alerts/${alertId}/acknowledge`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ acknowledgedBy: 'Fleet Manager' })
      });
      fetchAlerts(); // Refresh alerts
    } catch (error) {
      console.error('Error acknowledging alert:', error);
    }
  };

  const getSeverityColor = (severity: string) => {
    switch (severity.toLowerCase()) {
      case 'critical': return 'bg-red-100 text-red-800 border-red-300';
      case 'warning': return 'bg-yellow-100 text-yellow-800 border-yellow-300';
      case 'info': return 'bg-blue-100 text-blue-800 border-blue-300';
      default: return 'bg-gray-100 text-gray-800 border-gray-300';
    }
  };

  const filteredVessels = vessels.filter(vessel =>
    vessel.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    vessel.imo.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const filteredAlerts = alerts.filter(alert =>
    selectedSeverity === '' || alert.severity.toLowerCase() === selectedSeverity.toLowerCase()
  );

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <Ship className="mx-auto h-12 w-12 text-blue-500 animate-pulse" />
          <p className="mt-2 text-gray-600">Loading Maritime Fleet Data...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-2">
            <Ship className="h-8 w-8 text-blue-600" />
            Maritime Fleet Management
          </h1>
          <p className="text-gray-600">Real-time vessel tracking and fleet operations</p>
        </div>

        {/* Statistics Cards */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <Card>
            <CardContent className="p-6">
              <div className="flex items-center">
                <Ship className="h-8 w-8 text-blue-500" />
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Total Vessels</p>
                  <p className="text-2xl font-bold text-gray-900">{vessels.length}</p>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="p-6">
              <div className="flex items-center">
                <MapPin className="h-8 w-8 text-green-500" />
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Active Vessels</p>
                  <p className="text-2xl font-bold text-gray-900">
                    {vessels.filter(v => v.isActive).length}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="p-6">
              <div className="flex items-center">
                <AlertTriangle className="h-8 w-8 text-red-500" />
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Active Alerts</p>
                  <p className="text-2xl font-bold text-gray-900">{alerts.length}</p>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="p-6">
              <div className="flex items-center">
                <Clock className="h-8 w-8 text-orange-500" />
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Critical Alerts</p>
                  <p className="text-2xl font-bold text-gray-900">
                    {alerts.filter(a => a.severity === 'CRITICAL').length}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Filters and Search */}
        <div className="flex flex-col md:flex-row gap-4 mb-6">
          <div className="flex-1">
            <Input
              placeholder="Search vessels by name or IMO..."
              value={searchTerm}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) => setSearchTerm(e.target.value)}
              className="w-full"
            />
          </div>
          <div>
            <select
              value={selectedSeverity}
              onChange={(e) => setSelectedSeverity(e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">All Alert Severities</option>
              <option value="critical">Critical</option>
              <option value="warning">Warning</option>
              <option value="info">Info</option>
            </select>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* Vessel Fleet */}
          <div>
            <Card>
              <CardHeader>
                <h2 className="text-xl font-semibold">Fleet Overview</h2>
              </CardHeader>
              <CardContent>
                <div className="space-y-4 max-h-96 overflow-y-auto">
                  {filteredVessels.map((vessel) => (
                    <div key={vessel.id} className="p-4 border rounded-lg hover:bg-gray-50">
                      <div className="flex justify-between items-start">
                        <div className="flex-1">
                          <div className="flex items-center gap-2">
                            <h3 className="font-semibold">{vessel.name}</h3>
                            {vessel.isActive ? (
                              <Badge className="bg-green-100 text-green-800">Active</Badge>
                            ) : (
                              <Badge className="bg-gray-100 text-gray-800">Inactive</Badge>
                            )}
                          </div>
                          <p className="text-sm text-gray-600">IMO: {vessel.imo}</p>
                          <p className="text-sm text-gray-600">Call Sign: {vessel.callSign}</p>
                          <p className="text-sm text-gray-600">Type: {vessel.vesselType}</p>
                          <p className="text-sm text-gray-600">Flag: {vessel.flag}</p>
                          
                          {vessel.lastPosition && (
                            <div className="mt-2 text-sm text-gray-500">
                              <div className="flex items-center gap-1">
                                <MapPin className="h-3 w-3" />
                                {vessel.lastPosition.latitude.toFixed(4)}, {vessel.lastPosition.longitude.toFixed(4)}
                                {vessel.lastPosition.speed && (
                                  <span className="ml-2">
                                    <Fuel className="h-3 w-3 inline" /> {vessel.lastPosition.speed.toFixed(1)} kn
                                  </span>
                                )}
                              </div>
                              <p className="text-xs mt-1">
                                Last update: {new Date(vessel.lastPosition.timestamp).toLocaleString()}
                              </p>
                            </div>
                          )}
                        </div>
                        
                        {vessel.unacknowledgedAlerts > 0 && (
                          <div className="flex items-center gap-1 text-red-600">
                            <AlertTriangle className="h-4 w-4" />
                            <span className="text-sm font-semibold">{vessel.unacknowledgedAlerts}</span>
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Active Alerts */}
          <div>
            <Card>
              <CardHeader>
                <h2 className="text-xl font-semibold">Active Alerts</h2>
              </CardHeader>
              <CardContent>
                <div className="space-y-4 max-h-96 overflow-y-auto">
                  {filteredAlerts.map((alert) => (
                    <div key={alert.id} className="p-4 border rounded-lg">
                      <div className="flex justify-between items-start">
                        <div className="flex-1">
                          <div className="flex items-center gap-2 mb-2">
                            <Badge className={getSeverityColor(alert.severity)}>
                              {alert.severity}
                            </Badge>
                            <span className="text-sm text-gray-600">{alert.alertType}</span>
                          </div>
                          
                          <h3 className="font-semibold text-sm">{alert.vesselName || 'Unknown Vessel'}</h3>
                          <p className="text-sm text-gray-600 mb-2">{alert.message}</p>
                          
                          <p className="text-xs text-gray-500">
                            {new Date(alert.timestamp).toLocaleString()}
                          </p>
                        </div>
                        
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={() => acknowledgeAlert(alert.id)}
                          className="ml-4"
                        >
                          Acknowledge
                        </Button>
                      </div>
                    </div>
                  ))}
                  
                  {filteredAlerts.length === 0 && (
                    <div className="text-center py-8 text-gray-500">
                      <AlertTriangle className="mx-auto h-12 w-12 text-gray-300" />
                      <p className="mt-2">No active alerts</p>
                    </div>
                  )}
                </div>
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    </div>
  );
};

export default MaritimeFleetDashboard;