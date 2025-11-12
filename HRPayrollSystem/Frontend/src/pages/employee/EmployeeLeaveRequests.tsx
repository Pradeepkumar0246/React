import React, { useEffect, useState } from 'react';
import { Box, Button, Chip, Dialog, DialogActions, DialogContent, DialogTitle, FormControl, InputLabel, MenuItem, Paper, Select, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Typography, Checkbox } from '@mui/material';
import { Add, Visibility } from '@mui/icons-material';
import { useAuth } from '../../auth/AuthContext';
import { leaveRequestService } from '../../services/leaveRequestService';
import { leaveBalanceService } from '../../services/leaveBalanceService';

type LeaveRequest = {
  leaveRequestId: number;
  employeeId: string;
  employeeName: string;
  leaveType: string;
  fromDate: string;
  toDate: string;
  numberOfDays: number;
  isHalfDay: boolean;
  halfDayPeriod?: string;
  reason?: string;
  status: string;
  approvedBy?: string;
};

type LeaveBalance = {
  leaveType: string;
  allocatedDays: number;
  usedDays: number;
  remainingDays: number;
  year: number;
};

type FormState = { leaveType: string; fromDate: string; toDate: string; isHalfDay: boolean; halfDayPeriod: string; reason: string };

const EmployeeLeaveRequests: React.FC = () => {
  const { user } = useAuth();
  const [leaveRequests, setLeaveRequests] = useState<LeaveRequest[]>([]);
  const [loading, setLoading] = useState(false);
  const [openForm, setOpenForm] = useState(false);
  const [openBalance, setOpenBalance] = useState(false);
  const [balanceData, setBalanceData] = useState<LeaveBalance[]>([]);
  const [form, setForm] = useState<FormState>({ leaveType: 'Casual', fromDate: '', toDate: '', isHalfDay: false, halfDayPeriod: '', reason: '' });

  useEffect(() => { if (user?.employeeId) loadData(); }, [user?.employeeId]);

  const loadData = async () => {
    try {
      setLoading(true);
      const data = await leaveRequestService.getByEmployee(user?.employeeId || '');
      setLeaveRequests(data || []);
    } catch (err) { console.error('Load error:', err); } finally { setLoading(false); }
  };

  const handleSubmit = async () => {
    try {
      const data = { employeeId: user?.employeeId || '', ...form, halfDayPeriod: form.isHalfDay ? form.halfDayPeriod : undefined };
      await leaveRequestService.create(data);
      await loadData(); setOpenForm(false);
    } catch (err) { console.error('Save failed:', err); }
  };

  const handleViewBalance = async () => {
    try {
      const balances = await leaveBalanceService.getByEmployee(user?.employeeId || '', new Date().getFullYear());
      setBalanceData(balances || []);
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
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>My Leave Requests</Typography>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button variant="contained" startIcon={<Add />} onClick={() => setOpenForm(true)}>Request Leave</Button>
          <Button variant="outlined" startIcon={<Visibility />} onClick={handleViewBalance}>View Balance</Button>
        </Box>
        <Typography variant="body2" color="text.secondary">{leaveRequests.length} requests found</Typography>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Leave Type</TableCell><TableCell>From Date</TableCell><TableCell>To Date</TableCell><TableCell>Days</TableCell><TableCell>Reason</TableCell><TableCell>Status</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {leaveRequests.map((lr) => (
              <TableRow key={lr.leaveRequestId}>
                <TableCell>{lr.leaveType}{lr.isHalfDay && <Chip label={lr.halfDayPeriod} size="small" sx={{ ml: 1 }} />}</TableCell>
                <TableCell>{new Date(lr.fromDate).toLocaleDateString()}</TableCell>
                <TableCell>{new Date(lr.toDate).toLocaleDateString()}</TableCell>
                <TableCell>{lr.numberOfDays}</TableCell>
                <TableCell>{lr.reason || 'No reason provided'}</TableCell>
                <TableCell><Chip label={lr.status} color={getStatusColor(lr.status) as 'primary' | 'secondary' | 'success' | 'error' | 'info' | 'warning' | 'default'} size="small" /></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={openForm} onClose={() => setOpenForm(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Request Leave</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
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
          <Button variant="contained" onClick={handleSubmit}>Submit</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={openBalance} onClose={() => setOpenBalance(false)} maxWidth="md" fullWidth>
        <DialogTitle>Leave Balance - {user?.name}</DialogTitle>
        <DialogContent>
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Leave Type</TableCell><TableCell>Allocated</TableCell><TableCell>Used</TableCell><TableCell>Remaining</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {balanceData.map((balance) => (
                  <TableRow key={balance.leaveType}>
                    <TableCell>{balance.leaveType}</TableCell>
                    <TableCell>{balance.allocatedDays}</TableCell>
                    <TableCell>{balance.usedDays}</TableCell>
                    <TableCell>{balance.remainingDays}</TableCell>
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
    </Box>
  );
};

export default EmployeeLeaveRequests;