import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { ShieldAlert, Activity, RefreshCw, Server, AlertTriangle } from 'lucide-react';

const API_BASE_URL = 'http://localhost:5253';

export default function App() {
  const [logs, setLogs] = useState([]);
  const [securityEvents, setSecurityEvents] = useState([]);
  const [loading, setLoading] = useState(false);

  const fetchData = async () => {
    setLoading(true);
    try {
      const logsRes = await axios.get(`${API_BASE_URL}/api/dashboard/logs`);
      const eventsRes = await axios.get(`${API_BASE_URL}/api/dashboard/security-events`);

      setLogs(logsRes.data);
      setSecurityEvents(eventsRes.data);
    } catch (error) {
      console.error('Veri çekme hatası:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
    const interval = setInterval(fetchData, 5000); // 5 saniyede bir otomatik yenile
    return () => clearInterval(interval);
  }, []);

  return (
    <div style={styles.container}>
      {/* Header */}
      <header style={styles.header}>
        <div style={styles.brand}>
          <Server size={28} color="#3b82f6" />
          <h1 style={styles.title}>API Güvenlik & Trafik Analiz Paneli</h1>
        </div>
        <button onClick={fetchData} style={styles.refreshBtn} disabled={loading}>
          <RefreshCw size={16} className={loading ? 'spin' : ''} />
          {loading ? ' Yenileniyor...' : ' Yenile'}
        </button>
      </header>

      {/* Metrics Grid */}
      <div style={styles.gridStats}>
        <div style={styles.cardStat}>
          <Activity size={24} color="#10b981" />
          <div>
            <div style={styles.statNumber}>{logs.length}</div>
            <div style={styles.statLabel}>Toplam İzlenen İstek</div>
          </div>
        </div>
        <div style={styles.cardStat}>
          <ShieldAlert size={24} color="#ef4444" />
          <div>
            <div style={styles.statNumber}>{securityEvents.length}</div>
            <div style={styles.statLabel}>Engellenen Güvenlik İhlali</div>
          </div>
        </div>
      </div>

      {/* Tables Section */}
      <div style={styles.tablesContainer}>
        {/* Security Events Table */}
        <div style={styles.cardTable}>
          <h2 style={styles.tableHeader}>
            <AlertTriangle size={20} color="#ef4444" /> Engellenen Saldırılar (Security Events)
          </h2>
          <div style={styles.tableWrapper}>
            <table style={styles.table}>
              <thead>
                <tr>
                  <th>Zaman</th>
                  <th>Olay Tipi</th>
                  <th>Açıklama</th>
                </tr>
              </thead>
              <tbody>
                {securityEvents.length === 0 ? (
                  <tr><td colSpan="3" style={styles.empty}>Tehdit tespit edilmedi.</td></tr>
                ) : (
                  securityEvents.map((evt) => (
                    <tr key={evt.id} style={styles.dangerRow}>
                      <td>{new Date(evt.createdAt).toLocaleTimeString()}</td>
                      <td><span style={styles.badgeDanger}>{evt.eventType}</span></td>
                      <td>{evt.description}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>

        {/* Live Traffic Logs Table */}
        <div style={styles.cardTable}>
          <h2 style={styles.tableHeader}>
            <Activity size={20} color="#3b82f6" /> Canlı Trafik Logları (Request Logs)
          </h2>
          <div style={styles.tableWrapper}>
            <table style={styles.table}>
              <thead>
                <tr>
                  <th>Zaman</th>
                  <th>IP Adresi</th>
                  <th>Endpoint</th>
                  <th>Durum</th>
                  <th>Süre</th>
                </tr>
              </thead>
              <tbody>
                {logs.length === 0 ? (
                  <tr><td colSpan="5" style={styles.empty}>Henüz istek kaydı yok.</td></tr>
                ) : (
                  logs.map((log) => (
                    <tr key={log.id}>
                      <td>{new Date(log.timestamp).toLocaleTimeString()}</td>
                      <td><code>{log.clientIp}</code></td>
                      <td><code>{log.endpoint}</code></td>
                      <td>
                        <span style={log.statusCode === 429 ? styles.badgeDanger : styles.badgeSuccess}>
                          {log.statusCode}
                        </span>
                      </td>
                      <td>{log.durationMs} ms</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}

// Inline Minimalist Dashboard Stilleri
const styles = {
  container: { fontFamily: 'Segoe UI, sans-serif', backgroundColor: '#0f172a', color: '#f8fafc', minHeight: '100vh', padding: '24px' },
  header: { display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '24px', borderBottom: '1px solid #1e293b', paddingBottom: '16px' },
  brand: { display: 'flex', alignItems: 'center', gap: '12px' },
  title: { fontSize: '20px', fontWeight: '600', margin: 0 },
  refreshBtn: { display: 'flex', alignItems: 'center', gap: '8px', backgroundColor: '#1e293b', color: '#94a3b8', border: '1px solid #334155', padding: '8px 16px', borderRadius: '6px', cursor: 'pointer' },
  gridStats: { display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '16px', marginBottom: '24px' },
  cardStat: { backgroundColor: '#1e293b', padding: '20px', borderRadius: '8px', display: 'flex', alignItems: 'center', gap: '16px', border: '1px solid #334155' },
  statNumber: { fontSize: '24px', fontWeight: 'bold' },
  statLabel: { color: '#94a3b8', fontSize: '13px' },
  tablesContainer: { display: 'flex', flexDirection: 'column', gap: '24px' },
  cardTable: { backgroundColor: '#1e293b', padding: '20px', borderRadius: '8px', border: '1px solid #334155' },
  tableHeader: { fontSize: '16px', margin: '0 0 16px 0', display: 'flex', alignItems: 'center', gap: '8px' },
  tableWrapper: { overflowX: 'auto' },
  table: { width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '14px' },
  empty: { textAlign: 'center', color: '#64748b', padding: '24px' },
  dangerRow: { backgroundColor: 'rgba(239, 68, 68, 0.05)' },
  badgeSuccess: { backgroundColor: 'rgba(16, 185, 129, 0.2)', color: '#10b981', padding: '2px 8px', borderRadius: '4px', fontWeight: 'bold' },
  badgeDanger: { backgroundColor: 'rgba(239, 68, 68, 0.2)', color: '#ef4444', padding: '2px 8px', borderRadius: '4px', fontWeight: 'bold' }
};