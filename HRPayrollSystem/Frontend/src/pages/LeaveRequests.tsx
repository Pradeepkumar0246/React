import React, { useEffect, useMemo, useState } from 'react';
import { Avatar, Box, Button, Chip, Container, Dialog, DialogActions, DialogContent, DialogTitle, FormControl, IconButton, InputLabel, MenuItem, Paper, Select, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Tooltip, Typography, Checkbox } from '@mui/material';
import { Add, Edit, BeachAccess, Check, Close, Search, Visibility } from '@mui/icons-material';
import { useAuth } from '../auth/AuthContext';
import { leaveRequestService } from '../services/leaveRequestService';
import { employeeService } from '../services/employeeService';
import { leaveBalanceService } from '../services/leaveBalanceService';
import type { LeaveRequest, LeaveRequestFilters } from '../types/leaveRequest';
import type { Employee } from '../types/employee';
type LeaveBalanceWithStats = {
  leaveType: string;
  allocatedDays: number;
  usedDays: number;
  remainingDays: number;
  year: number;
  usedThisMonth: number;
  pendingRequests: number;
};

type FormState = { employeeId: string; leaveType: string; fromDate: string; toDate: string; isHalfDay: boolean; halfDayPeriod: string; reason: string };

const LeaveRequests: React.FC = () => {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin' || user?.role === 'HR Manager';
  const [leaveRequests, setLeaveRequests] = useState<LeaveRequest[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(false);
  const [filters, setFilters] = useState<LeaveRequestFilters>({ search: '', employeeId: '', status: '', leaveType: '', fromDate: '', toDate: '' });
  const [openForm, setOpenForm] = useState(false);
  const [editLeaveRequest, setEditLeaveRequest] = useState<LeaveRequest | null>(null);
  const [form, setForm] = useState<FormState>({ employeeId: '', leaveType: 'Casual', fromDate: '', toDate: '', isHalfDay: false, halfDayPeriod: '', reason: '' });
  const [openBalance, setOpenBalance] = useState(false);
  const [balanceData, setBalanceData] = useState<LeaveBalanceWithStats[]>([]);
  const [balanceEmployee, setBalanceEmployee] = useState<string>('');

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      if (isAdmin) {
        const [leaveData, employeesData] = await Promise.all([leaveRequestService.getAll(), employeeService.getAllEmployees()]);
        setLeaveRequests(leaveData ?? []); setEmployees(employeesData ?? []);
      } else {
        const leaveData = await leaveRequestService.getByEmployee(user?.employeeId || '');
        setLeaveRequests(leaveData ?? []); setEmployees([]);
      }
    } catch (err) { console.error('Load error:', err); } finally { setLoading(false); }
  };

  const filtered = useMemo(() => {
    const s = (filters.search || '').toLowerCase();
    return leaveRequests.filter((lr) => {
      const matchSearch = lr.employeeName?.toLowerCase().includes(s) || lr.employeeId?.toLowerCase().includes(s);
      return matchSearch && (!filters.employeeId || lr.employeeId === filters.employeeId) && (!filters.status || lr.status === filters.status) && (!filters.leaveType || lr.leaveType === filters.leaveType) && (!filters.fromDate || new Date(lr.fromDate) >= new Date(filters.fromDate)) && (!filters.toDate || new Date(lr.toDate) <= new Date(filters.toDate));
    });
  }, [leaveRequests, filters]);

  const handleOpenForm = (lr?: LeaveRequest) => {
    setEditLeaveRequest(lr ?? null);
    setForm(lr ? { employeeId: lr.employeeId, leaveType: lr.leaveType, fromDate: lr.fromDate.split('T')[0], toDate: lr.toDate.split('T')[0], isHalfDay: lr.isHalfDay, halfDayPeriod: lr.halfDayPeriod || '', reason: lr.reason || '' } : { employeeId: user?.employeeId || '', leaveType: 'Casual', fromDate: '', toDate: '', isHalfDay: false, halfDayPeriod: '', reason: '' });
    setOpenForm(true);
  };

  const handleSubmit = async () => {
    try {
      const data = { ...form, halfDayPeriod: form.isHalfDay ? form.halfDayPeriod : undefined };
      if (editLeaveRequest) {
        await leaveRequestService.update(editLeaveRequest.leaveRequestId, { leaveRequestId: editLeaveRequest.leaveRequestId, ...data, status: editLeaveRequest.status, approvedBy: editLeaveRequest.approvedBy });
      } else {
        await leaveRequestService.create(data);
      }
      await loadData(); setOpenForm(false);
    } catch (err) { console.error('Save failed:', err); }
  };

  const handleStatusUpdate = async (id: number, status: string) => {
    try { await leaveRequestService.updateStatus(id, status, user?.employeeId); await loadData(); } catch (err) { console.error('Status update failed:', err); }
  };

  const handleViewBalance = async (employeeId: string, employeeName: string) => {
    if (!isAdmin && employeeId !== user?.employeeId) return;
    try {
      const balances = await leaveBalanceService.getByEmployee(employeeId, new Date().getFullYear());
      const currentMonth = new Date().getMonth() + 1;
      const currentYear = new Date().getFullYear();
      
      const enhanced = await Promise.all(balances.map(async (balance) => {
        const monthlyUsed = leaveRequests.filter(lr => 
          lr.employeeId === employeeId && 
          lr.leaveType === balance.leaveType && 
          lr.status === 'Approved' &&
          new Date(lr.fromDate).getMonth() + 1 === currentMonth &&
          new Date(lr.fromDate).getFullYear() === currentYear
        ).reduce((sum, lr) => sum + lr.numberOfDays, 0);
        
        const pending = leaveRequests.filter(lr => 
          lr.employeeId === employeeId && 
          lr.leaveType === balance.leaveType && 
          lr.status === 'Pending'
        ).reduce((sum, lr) => sum + lr.numberOfDays, 0);
        
        return { ...balance, usedThisMonth: monthlyUsed, pendingRequests: pending };
      }));
      
      setBalanceData(enhanced);
      setBalanceEmployee(employeeName);
      setOpenBalance(true);
    } catch (err) { console.error('Failed to load balance:', err); }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Approved': return 'success';
      case 'Rejected': return 'error';
      case 'Pending': return 'warning';
      default: return 'default';
    }
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>Leave Requests</Typography>

      <Paper sx={{ p: 2, mb: 3 }}>
        <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 2, alignItems: 'center' }}>
          <TextField placeholder="Search by name or ID" value={filters.search} onChange={(e) => setFilters(s => ({ ...s, search: e.target.value }))} InputProps={{ startAdornment: <Search sx={{ mr: 1, color: 'text.secondary' }} /> }} />
          <FormControl><InputLabel>Employee</InputLabel><Select value={filters.employeeId} label="Employee" onChange={(e) => setFilters(s => ({ ...s, employeeId: e.target.value }))}><MenuItem value="">All</MenuItem>{employees.map((e) => <MenuItem key={e.employeeId} value={e.employeeId}>{e.fullName}</MenuItem>)}</Select></FormControl>
          <FormControl><InputLabel>Status</InputLabel><Select value={filters.status} label="Status" onChange={(e) => setFilters(s => ({ ...s, status: e.target.value }))}><MenuItem value="">All</MenuItem><MenuItem value="Pending">Pending</MenuItem><MenuItem value="Approved">Approved</MenuItem><MenuItem value="Rejected">Rejected</MenuItem></Select></FormControl>
          <FormControl><InputLabel>Leave Type</InputLabel><Select value={filters.leaveType} label="Leave Type" onChange={(e) => setFilters(s => ({ ...s, leaveType: e.target.value }))}><MenuItem value="">All</MenuItem><MenuItem value="Casual">Casual</MenuItem><MenuItem value="Sick">Sick</MenuItem><MenuItem value="Earned">Earned</MenuItem><MenuItem value="Maternity">Maternity</MenuItem><MenuItem value="Paternity">Paternity</MenuItem><MenuItem value="Emergency">Emergency</MenuItem><MenuItem value="LOP">LOP</MenuItem></Select></FormControl>
          <TextField type="date" label="From" value={filters.fromDate} onChange={(e) => setFilters(s => ({ ...s, fromDate: e.target.value }))} InputLabelProps={{ shrink: true }} />
          <TextField type="date" label="To" value={filters.toDate} onChange={(e) => setFilters(s => ({ ...s, toDate: e.target.value }))} InputLabelProps={{ shrink: true }} />
        </Box>
      </Paper>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Box>
          <Button variant="contained" startIcon={<Add />} onClick={() => handleOpenForm()}>Request Leave</Button>
        </Box>
        <Typography variant="body2" color="text.secondary">{filtered.length} requests found</Typography>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Employee</TableCell><TableCell>Leave Type</TableCell><TableCell>From Date</TableCell><TableCell>To Date</TableCell><TableCell>Days</TableCell><TableCell>Reason</TableCell><TableCell>Status</TableCell><TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filtered.map((lr) => (
              <TableRow key={lr.leaveRequestId}>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Avatar sx={{ bgcolor: 'secondary.main' }}><BeachAccess /></Avatar>
                    <Box><Typography variant="subtitle2">{lr.employeeName}</Typography><Typography variant="body2" color="text.secondary">{lr.employeeId}</Typography></Box>
                  </Box>
                </TableCell>
                <TableCell>{lr.leaveType}{lr.isHalfDay && <Chip label={lr.halfDayPeriod} size="small" sx={{ ml: 1 }} />}</TableCell>
                <TableCell>{new Date(lr.fromDate).toLocaleDateString()}</TableCell>
                <TableCell>{new Date(lr.toDate).toLocaleDateString()}</TableCell>
                <TableCell>{lr.numberOfDays}</TableCell>
                <TableCell>{lr.reason || 'No reason provided'}</TableCell>
                <TableCell><Chip label={lr.status} color={getStatusColor(lr.status) as any} size="small" /></TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    {(lr.employeeId === user?.employeeId || isAdmin) && lr.status === 'Pending' && <Tooltip title="Edit"><IconButton size="small" onClick={() => handleOpenForm(lr)}><Edit /></IconButton></Tooltip>}
                    {isAdmin && lr.status === 'Pending' && (
                      <>
                        <Tooltip title="Approve"><IconButton size="small" color="success" onClick={() => handleStatusUpdate(lr.leaveRequestId, 'Approved')}><Check /></IconButton></Tooltip>
                        <Tooltip title="Reject"><IconButton size="small" color="error" onClick={() => handleStatusUpdate(lr.leaveRequestId, 'Rejected')}><Close /></IconButton></Tooltip>
                      </>
                    )}
                    {(isAdmin || lr.employeeId === user?.employeeId) && <Tooltip title="View Balance"><IconButton size="small" onClick={() => handleViewBalance(lr.employeeId, lr.employeeName)}><Visibility /></IconButton></Tooltip>}
                  </Box>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={openForm} onClose={() => setOpenForm(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editLeaveRequest ? 'Edit Leave Request' : 'Request Leave'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <FormControl><InputLabel>Employee</InputLabel><Select value={form.employeeId} label="Employee" onChange={(e) => setForm(s => ({ ...s, employeeId: e.target.value }))} disabled={!isAdmin}>{employees.map((e) => <MenuItem key={e.employeeId} value={e.employeeId}>{e.fullName}</MenuItem>)}</Select></FormControl>
            <FormControl><InputLabel>Leave Type</InputLabel><Select value={form.leaveType} label="Leave Type" onChange={(e) => setForm(s => ({ ...s, leaveType: e.target.value }))}><MenuItem value="Casual">Casual</MenuItem><MenuItem value="Sick">Sick</MenuItem><MenuItem value="Earned">Earned</MenuItem><MenuItem value="Maternity">Maternity</MenuItem><MenuItem value="Paternity">Paternity</MenuItem><MenuItem value="Emergency">Emergency</MenuItem><MenuItem value="LOP">LOP</MenuItem></Select></FormControl>
            <TextField type="date" label="From Date" value={form.fromDate} onChange={(e) => setForm(s => ({ ...s, fromDate: e.target.value }))} InputLabelProps={{ shrink: true }} />
            <TextField type="date" label="To Date" value={form.toDate} onChange={(e) => setForm(s => ({ ...s, toDate: e.target.value }))} InputLabelProps={{ shrink: true }} />
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Checkbox checked={form.isHalfDay} onChange={(e) => setForm(s => ({ ...s, isHalfDay: e.target.checked }))} />
              <Typography>Half Day</Typography>
              {form.isHalfDay && <FormControl sx={{ minWidth: 120 }}><InputLabel>Period</InputLabel><Select value={form.halfDayPeriod} label="Period" onChange={(e) => setForm(s => ({ ...s, halfDayPeriod: e.target.value }))}><MenuItem value="Morning">Morning</MenuItem><MenuItem value="Afternoon">Afternoon</MenuItem></Select></FormControl>}
            </Box>
            <TextField label="Reason" multiline rows={3} value={form.reason} onChange={(e) => setForm(s => ({ ...s, reason: e.target.value }))} />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenForm(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSubmit}>{editLeaveRequest ? 'Update' : 'Submit'}</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={openBalance} onClose={() => setOpenBalance(false)} maxWidth="md" fullWidth>
        <DialogTitle>Leave Balance - {balanceEmployee}</DialogTitle>
        <DialogContent>
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Leave Type</TableCell><TableCell>Allocated</TableCell><TableCell>Used</TableCell><TableCell>Remaining</TableCell><TableCell>Used This Month</TableCell><TableCell>Pending</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {balanceData.map((balance) => (
                  <TableRow key={balance.leaveType}>
                    <TableCell>{balance.leaveType}</TableCell>
                    <TableCell>{balance.allocatedDays}</TableCell>
                    <TableCell>{balance.usedDays}</TableCell>
                    <TableCell>{balance.remainingDays}</TableCell>
                    <TableCell>{balance.usedThisMonth}</TableCell>
                    <TableCell>{balance.pendingRequests}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenBalance(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      {loading && <Typography sx={{ mt: 2 }}>Loading...</Typography>}
    </Container>
  );
};

export default LeaveRequests;