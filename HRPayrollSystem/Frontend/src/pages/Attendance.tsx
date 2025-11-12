import React, { useEffect, useMemo, useState } from 'react';
import { Avatar, Box, Button, Chip, Container, Dialog, DialogActions, DialogContent, DialogTitle, FormControl, IconButton, InputLabel, MenuItem, Paper, Select, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Tooltip, Typography } from '@mui/material';
import { Add, Edit, AccessTime, Login, Logout, Search } from '@mui/icons-material';
import { useAuth } from '../auth/AuthContext';
import { attendanceService } from '../services/attendanceService';
import { employeeService } from '../services/employeeService';
import type { Attendance, AttendanceFilters } from '../types/attendance';
import type { Employee } from '../types/employee';

type FormState = { employeeId: string; date: string; loginTime: string; logoutTime: string; status: string };

const AttendancePage: React.FC = () => {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin' || user?.role === 'HR Manager';
  const [attendance, setAttendance] = useState<Attendance[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(false);
  const [filters, setFilters] = useState<AttendanceFilters>({ search: '', employeeId: '', status: '', fromDate: '', toDate: '' });
  const [openForm, setOpenForm] = useState(false);
  const [editAttendance, setEditAttendance] = useState<Attendance | null>(null);
  const [form, setForm] = useState<FormState>({ employeeId: '', date: '', loginTime: '', logoutTime: '', status: 'Present' });

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [attendanceData, employeesData] = await Promise.all([attendanceService.getAll(), employeeService.getAllEmployees()]);
      setAttendance(attendanceData ?? []); setEmployees(employeesData ?? []);
    } catch (err) { console.error('Load error:', err); } finally { setLoading(false); }
  };

  const filtered = useMemo(() => {
    const s = (filters.search || '').toLowerCase();
    return attendance.filter((a) => {
      const matchSearch = a.employeeName?.toLowerCase().includes(s) || a.employeeId?.toLowerCase().includes(s);
      return matchSearch && (!filters.employeeId || a.employeeId === filters.employeeId) && (!filters.status || a.status === filters.status) && (!filters.fromDate || new Date(a.date) >= new Date(filters.fromDate)) && (!filters.toDate || new Date(a.date) <= new Date(filters.toDate));
    });
  }, [attendance, filters]);

  const handleOpenForm = (att?: Attendance) => {
    setEditAttendance(att ?? null);
    setForm(att ? { employeeId: att.employeeId, date: att.date.split('T')[0], loginTime: att.loginTime, logoutTime: att.logoutTime || '', status: att.status } : { employeeId: '', date: new Date().toISOString().split('T')[0], loginTime: '', logoutTime: '', status: 'Present' });
    setOpenForm(true);
  };

  const handleSubmit = async () => {
    try {
      const formatTime = (time: string) => time.length === 5 ? time + ':00' : time;
      const data = { ...form, loginTime: formatTime(form.loginTime), logoutTime: form.logoutTime ? formatTime(form.logoutTime) : undefined };
      if (editAttendance) {
        await attendanceService.update(editAttendance.attendanceId, { attendanceId: editAttendance.attendanceId, ...data });
      } else {
        await attendanceService.create(data);
      }
      await loadData(); setOpenForm(false);
    } catch (err) { console.error('Save failed:', err); }
  };

  const handleCheckIn = async () => {
    if (!user?.employeeId) { alert('Employee ID not available'); return; }
    try { await attendanceService.checkIn(user.employeeId); await loadData(); } catch (err: any) { alert(err.response?.data?.message || 'Check-in failed'); }
  };

  const handleCheckOut = async () => {
    if (!user?.employeeId) { alert('Employee ID not available'); return; }
    try { await attendanceService.checkOut(user.employeeId); await loadData(); } catch (err: any) { alert(err.response?.data?.message || 'Check-out failed'); }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Present': return 'success';
      case 'Absent': return 'error';
      case 'Leave': return 'warning';
      case 'Holiday': return 'info';
      default: return 'default';
    }
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>Attendance</Typography>

      <Paper sx={{ p: 2, mb: 3 }}>
        <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 2, alignItems: 'center' }}>
          <TextField placeholder="Search by name or ID" value={filters.search} onChange={(e) => setFilters(s => ({ ...s, search: e.target.value }))} InputProps={{ startAdornment: <Search sx={{ mr: 1, color: 'text.secondary' }} /> }} />
          <FormControl><InputLabel>Employee</InputLabel><Select value={filters.employeeId} label="Employee" onChange={(e) => setFilters(s => ({ ...s, employeeId: e.target.value }))}><MenuItem value="">All</MenuItem>{employees.map((e) => <MenuItem key={e.employeeId} value={e.employeeId}>{e.fullName}</MenuItem>)}</Select></FormControl>
          <FormControl><InputLabel>Status</InputLabel><Select value={filters.status} label="Status" onChange={(e) => setFilters(s => ({ ...s, status: e.target.value }))}><MenuItem value="">All</MenuItem><MenuItem value="Present">Present</MenuItem><MenuItem value="Absent">Absent</MenuItem><MenuItem value="Leave">Leave</MenuItem><MenuItem value="Holiday">Holiday</MenuItem></Select></FormControl>
          <TextField type="date" label="From" value={filters.fromDate} onChange={(e) => setFilters(s => ({ ...s, fromDate: e.target.value }))} InputLabelProps={{ shrink: true }} />
          <TextField type="date" label="To" value={filters.toDate} onChange={(e) => setFilters(s => ({ ...s, toDate: e.target.value }))} InputLabelProps={{ shrink: true }} />
        </Box>
      </Paper>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Box>
          {isAdmin && <Button variant="contained" startIcon={<Add />} sx={{ mr: 2 }} onClick={() => handleOpenForm()}>Add Attendance</Button>}
          <Button variant="outlined" startIcon={<Login />} sx={{ mr: 2 }} onClick={handleCheckIn} disabled={!user?.employeeId}>Check In</Button>
          <Button variant="outlined" startIcon={<Logout />} onClick={handleCheckOut} disabled={!user?.employeeId}>Check Out</Button>
        </Box>
        <Typography variant="body2" color="text.secondary">{filtered.length} records found</Typography>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Employee</TableCell><TableCell>Date</TableCell><TableCell>Login Time</TableCell><TableCell>Logout Time</TableCell><TableCell>Working Hours</TableCell><TableCell>Status</TableCell><TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filtered.map((att) => (
              <TableRow key={att.attendanceId}>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Avatar sx={{ bgcolor: 'primary.main' }}><AccessTime /></Avatar>
                    <Box><Typography variant="subtitle2">{att.employeeName}</Typography><Typography variant="body2" color="text.secondary">{att.employeeId}</Typography></Box>
                  </Box>
                </TableCell>
                <TableCell>{new Date(att.date).toLocaleDateString()}</TableCell>
                <TableCell>{att.loginTime}</TableCell>
                <TableCell>{att.logoutTime || 'Not logged out'}</TableCell>
                <TableCell>{att.workingHours.toFixed(2)} hrs</TableCell>
                <TableCell><Chip label={att.status} color={getStatusColor(att.status) as any} size="small" /></TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    {isAdmin && <Tooltip title="Edit"><IconButton size="small" onClick={() => handleOpenForm(att)}><Edit /></IconButton></Tooltip>}
                  </Box>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={openForm} onClose={() => setOpenForm(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editAttendance ? 'Edit Attendance' : 'Add Attendance'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <FormControl><InputLabel>Employee</InputLabel><Select value={form.employeeId} label="Employee" onChange={(e) => setForm(s => ({ ...s, employeeId: e.target.value }))}>{employees.map((e) => <MenuItem key={e.employeeId} value={e.employeeId}>{e.fullName}</MenuItem>)}</Select></FormControl>
            <TextField type="date" label="Date" value={form.date} onChange={(e) => setForm(s => ({ ...s, date: e.target.value }))} InputLabelProps={{ shrink: true }} />
            <TextField type="time" label="Login Time" value={form.loginTime} onChange={(e) => setForm(s => ({ ...s, loginTime: e.target.value }))} InputLabelProps={{ shrink: true }} />
            <TextField type="time" label="Logout Time" value={form.logoutTime} onChange={(e) => setForm(s => ({ ...s, logoutTime: e.target.value }))} InputLabelProps={{ shrink: true }} />
            <FormControl><InputLabel>Status</InputLabel><Select value={form.status} label="Status" onChange={(e) => setForm(s => ({ ...s, status: e.target.value }))}><MenuItem value="Present">Present</MenuItem><MenuItem value="Absent">Absent</MenuItem><MenuItem value="Leave">Leave</MenuItem><MenuItem value="Holiday">Holiday</MenuItem></Select></FormControl>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenForm(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSubmit}>{editAttendance ? 'Update' : 'Create'}</Button>
        </DialogActions>
      </Dialog>

      {loading && <Typography sx={{ mt: 2 }}>Loading...</Typography>}
    </Container>
  );
};

export default AttendancePage;