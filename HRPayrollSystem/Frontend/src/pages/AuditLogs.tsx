import React, { useEffect, useMemo, useState } from 'react';
import { Avatar, Box, Button, Chip, Container, FormControl, IconButton, InputLabel, MenuItem, Paper, Select, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Tooltip, Typography, Collapse } from '@mui/material';
import { Assignment, ExpandMore, ExpandLess, Refresh, Search } from '@mui/icons-material';
import { useAuth } from '../auth/AuthContext';
import { auditLogService } from '../services/auditLogService';
import { employeeService } from '../services/employeeService';
import type { AuditLog, AuditLogFilters } from '../types/auditLog';
import type { Employee } from '../types/employee';

const AuditLogsPage: React.FC = () => {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin' || user?.role === 'HR Manager';
  const [auditLogs, setAuditLogs] = useState<AuditLog[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(false);
  const [filters, setFilters] = useState<AuditLogFilters>({ search: '', employeeId: '', action: '', tableName: '', fromDate: '', toDate: '' });
  const [expandedRows, setExpandedRows] = useState<Set<number>>(new Set());

  const actions = ['Created', 'Updated', 'Deleted', 'Login', 'Logout'];
  const tableNames = ['Employee', 'Department', 'Role', 'Attendance', 'LeaveRequest', 'Payroll', 'Payslip', 'SalaryStructure', 'EmployeeDocument'];

  useEffect(() => { 
    if (isAdmin) loadData(); 
  }, [isAdmin]);

  const loadData = async () => {
    try {
      setLoading(true);
      const [auditData, employeesData] = await Promise.all([
        auditLogService.getAll(),
        employeeService.getAllEmployees()
      ]);
      setAuditLogs(auditData ?? []); setEmployees(employeesData ?? []);
    } catch (err) { console.error('Load error:', err); } finally { setLoading(false); }
  };

  const filtered = useMemo(() => {
    const s = (filters.search || '').toLowerCase();
    return auditLogs.filter((log) => {
      const matchSearch = log.employeeId?.toLowerCase().includes(s) || log.tableName?.toLowerCase().includes(s) || log.recordId?.toLowerCase().includes(s);
      const matchEmployee = !filters.employeeId || log.employeeId === filters.employeeId;
      const matchAction = !filters.action || log.action === filters.action;
      const matchTable = !filters.tableName || log.tableName === filters.tableName;
      const matchFromDate = !filters.fromDate || new Date(log.timestamp) >= new Date(filters.fromDate);
      const matchToDate = !filters.toDate || new Date(log.timestamp) <= new Date(filters.toDate + 'T23:59:59');
      return matchSearch && matchEmployee && matchAction && matchTable && matchFromDate && matchToDate;
    });
  }, [auditLogs, filters]);

  const toggleExpand = (auditId: number) => {
    const newExpanded = new Set(expandedRows);
    if (newExpanded.has(auditId)) {
      newExpanded.delete(auditId);
    } else {
      newExpanded.add(auditId);
    }
    setExpandedRows(newExpanded);
  };

  const getActionColor = (action: string) => {
    switch (action) {
      case 'Created': return 'success';
      case 'Updated': return 'info';
      case 'Deleted': return 'error';
      case 'Login': return 'primary';
      case 'Logout': return 'warning';
      default: return 'default';
    }
  };

  const formatJson = (jsonString?: string) => {
    if (!jsonString) return 'No data';
    try {
      const parsed = JSON.parse(jsonString);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return jsonString;
    }
  };

  if (!isAdmin) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Typography variant="h4" color="error">Access Denied</Typography>
        <Typography>You don't have permission to view audit logs.</Typography>
      </Container>
    );
  }

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>Audit Logs</Typography>

      <Paper sx={{ p: 2, mb: 3 }}>
        <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 2, alignItems: 'center' }}>
          <TextField placeholder="Search by employee, table, or record ID" value={filters.search} onChange={(e) => setFilters(s => ({ ...s, search: e.target.value }))} InputProps={{ startAdornment: <Search sx={{ mr: 1, color: 'text.secondary' }} /> }} />
          <FormControl><InputLabel>Employee</InputLabel><Select value={filters.employeeId} label="Employee" onChange={(e) => setFilters(s => ({ ...s, employeeId: e.target.value }))}><MenuItem value="">All</MenuItem>{employees.map((e) => <MenuItem key={e.employeeId} value={e.employeeId}>{e.fullName}</MenuItem>)}</Select></FormControl>
          <FormControl><InputLabel>Action</InputLabel><Select value={filters.action} label="Action" onChange={(e) => setFilters(s => ({ ...s, action: e.target.value }))}><MenuItem value="">All</MenuItem>{actions.map((action) => <MenuItem key={action} value={action}>{action}</MenuItem>)}</Select></FormControl>
          <FormControl><InputLabel>Table</InputLabel><Select value={filters.tableName} label="Table" onChange={(e) => setFilters(s => ({ ...s, tableName: e.target.value }))}><MenuItem value="">All</MenuItem>{tableNames.map((table) => <MenuItem key={table} value={table}>{table}</MenuItem>)}</Select></FormControl>
          <TextField type="date" label="From Date" value={filters.fromDate} onChange={(e) => setFilters(s => ({ ...s, fromDate: e.target.value }))} InputLabelProps={{ shrink: true }} />
          <TextField type="date" label="To Date" value={filters.toDate} onChange={(e) => setFilters(s => ({ ...s, toDate: e.target.value }))} InputLabelProps={{ shrink: true }} />
        </Box>
      </Paper>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Box>
          <Button variant="outlined" startIcon={<Refresh />} onClick={loadData} disabled={loading}>Refresh</Button>
        </Box>
        <Typography variant="body2" color="text.secondary">{filtered.length} audit logs found</Typography>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Employee ID</TableCell><TableCell>Action</TableCell><TableCell>Table</TableCell><TableCell>Record ID</TableCell><TableCell>Timestamp</TableCell><TableCell>Details</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filtered.map((log) => (
              <React.Fragment key={log.auditId}>
                <TableRow>
                  <TableCell>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                      <Avatar sx={{ bgcolor: 'primary.main' }}><Assignment /></Avatar>
                      <Typography variant="subtitle2">{log.employeeId}</Typography>
                    </Box>
                  </TableCell>
                  <TableCell><Chip label={log.action} color={getActionColor(log.action) as any} size="small" /></TableCell>
                  <TableCell>{log.tableName}</TableCell>
                  <TableCell>{log.recordId}</TableCell>
                  <TableCell>
                    <Box>
                      <Typography variant="body2">{new Date(log.timestamp).toLocaleString()}</Typography>
                      <Typography variant="caption" color="text.secondary">{log.timeAgo}</Typography>
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Tooltip title="View Details">
                      <IconButton size="small" onClick={() => toggleExpand(log.auditId)}>
                        {expandedRows.has(log.auditId) ? <ExpandLess /> : <ExpandMore />}
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell colSpan={6} sx={{ p: 0 }}>
                    <Collapse in={expandedRows.has(log.auditId)}>
                      <Box sx={{ p: 2, bgcolor: 'grey.50' }}>
                        <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
                          <Box>
                            <Typography variant="subtitle2" gutterBottom>Old Values:</Typography>
                            <Paper sx={{ p: 1, bgcolor: 'background.paper', maxHeight: 200, overflow: 'auto' }}>
                              <pre style={{ margin: 0, fontSize: '12px', whiteSpace: 'pre-wrap' }}>
                                {formatJson(log.oldValues)}
                              </pre>
                            </Paper>
                          </Box>
                          <Box>
                            <Typography variant="subtitle2" gutterBottom>New Values:</Typography>
                            <Paper sx={{ p: 1, bgcolor: 'background.paper', maxHeight: 200, overflow: 'auto' }}>
                              <pre style={{ margin: 0, fontSize: '12px', whiteSpace: 'pre-wrap' }}>
                                {formatJson(log.newValues)}
                              </pre>
                            </Paper>
                          </Box>
                        </Box>
                      </Box>
                    </Collapse>
                  </TableCell>
                </TableRow>
              </React.Fragment>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {loading && <Typography sx={{ mt: 2 }}>Loading...</Typography>}
    </Container>
  );
};

export default AuditLogsPage;