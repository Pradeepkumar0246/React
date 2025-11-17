import React, { useEffect, useState } from 'react';
import { Box, Typography, Container, Card, CardContent, Button, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Chip, LinearProgress, CircularProgress } from '@mui/material';
import Grid from '@mui/material/Grid';
import { Login, Logout, AccessTime, BeachAccess, TrendingUp, CalendarToday } from '@mui/icons-material';
import { PieChart, Pie, Cell, ResponsiveContainer } from 'recharts';
import { useAuth } from '../auth/AuthContext';
import { attendanceService } from '../services/attendanceService';
import { leaveBalanceService } from '../services/leaveBalanceService';
import { dashboardColors } from '../theme/theme';
import type { Attendance } from '../types/attendance';
import type { LeaveBalance } from '../types/leaveBalance';

const EmployeeDashboard: React.FC = () => {
  const { user } = useAuth();
  const [attendance, setAttendance] = useState<Attendance[]>([]);
  const [leaveBalance, setLeaveBalance] = useState<LeaveBalance[]>([]);
  const [loading, setLoading] = useState(false);
  const [todayAttendance, setTodayAttendance] = useState<Attendance | null>(null);
  const [dashboardLoading, setDashboardLoading] = useState(true);

  useEffect(() => { if (user?.employeeId) loadDashboardData(); }, [user?.employeeId]);

  const loadDashboardData = async () => {
    try {
      setDashboardLoading(true);
      const [attendanceData, leaveData] = await Promise.all([
        attendanceService.getByEmployee(user?.employeeId || ''),
        leaveBalanceService.getByEmployee(user?.employeeId || '', new Date().getFullYear())
      ]);
      setAttendance(attendanceData || []);
      setLeaveBalance(leaveData || []);
      const today = new Date().toISOString().split('T')[0];
      setTodayAttendance(attendanceData?.find(a => a.date.split('T')[0] === today) || null);
    } catch (err) {
      console.error('Dashboard load error:', err);
    } finally {
      setDashboardLoading(false);
    }
  };

  const handleCheckIn = async () => {
    try {
      await attendanceService.checkIn(user?.employeeId || '');
      loadDashboardData();
    } catch (err) { console.error('Check-in failed:', err); }
  };

  const handleCheckOut = async () => {
    try {
      await attendanceService.checkOut(user?.employeeId || '');
      loadDashboardData();
    } catch (err) { console.error('Check-out failed:', err); }
  };

  const getStatusColor = (status: string) => {
    const colors = { Present: 'success', Absent: 'error', Leave: 'warning', Holiday: 'info' };
    return colors[status as keyof typeof colors] || 'default';
  };

  const currentMonth = new Date().getMonth();
  const monthlyAttendance = attendance.filter(a => new Date(a.date).getMonth() === currentMonth);
  const presentDays = monthlyAttendance.filter(a => a.status === 'Present').length;
  const totalWorkingDays = monthlyAttendance.length;
  const attendanceRate = totalWorkingDays > 0 ? (presentDays / totalWorkingDays * 100) : 0;
  const totalLeaveBalance = leaveBalance.reduce((sum, lb) => sum + lb.remainingDays, 0);
  const totalAllocated = leaveBalance.reduce((sum, lb) => sum + lb.allocatedDays, 0);
  const totalWorkingHours = monthlyAttendance.reduce((sum, a) => sum + (a.workingHours || 0), 0);
  const targetHours = totalWorkingDays * 8;
  
  const COLORS = ['#00C49F', '#FF8042'];
  const leaveChartData = [
    { name: 'Used', value: totalAllocated - totalLeaveBalance },
    { name: 'Available', value: totalLeaveBalance }
  ];

  if (dashboardLoading) return (
    <Container sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>Welcome, {user?.name}!</Typography>
      <LinearProgress sx={{ mb: 4 }} />
    </Container>
  );

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>Welcome, {user?.name}!</Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
        Your personal HR dashboard with insights and quick actions.
      </Typography>

      {/* Metric Cards */}
      <Grid container spacing={2} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} lg={3}>
          <Card sx={{ background: dashboardColors.blue.gradient, color: 'white' }}>
            <CardContent sx={{ p: 2 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography variant="h5" sx={{ fontWeight: 'bold' }}>{attendanceRate.toFixed(1)}%</Typography>
                  <Typography variant="body1">Attendance Rate</Typography>
                  <Typography variant="body2" sx={{ opacity: 0.9 }}>{presentDays}/{totalWorkingDays} days</Typography>
                </Box>
                <AccessTime sx={{ fontSize: 24 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} lg={3}>
          <Card sx={{ background: dashboardColors.green.gradient, color: 'white' }}>
            <CardContent sx={{ p: 2 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography variant="h5" sx={{ fontWeight: 'bold' }}>{totalLeaveBalance}</Typography>
                  <Typography variant="body1">Leave Balance</Typography>
                  <Typography variant="body2" sx={{ opacity: 0.9 }}>Days remaining</Typography>
                </Box>
                <BeachAccess sx={{ fontSize: 24 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} lg={3}>
          <Card sx={{ background: dashboardColors.blueGreen.gradient, color: 'white' }}>
            <CardContent sx={{ p: 2 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography variant="h5" sx={{ fontWeight: 'bold' }}>{totalWorkingHours.toFixed(1)}h</Typography>
                  <Typography variant="body1">Working Hours</Typography>
                  <Typography variant="body2" sx={{ opacity: 0.9 }}>This month</Typography>
                </Box>
                <TrendingUp sx={{ fontSize: 24 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} lg={3}>
          <Card sx={{ background: dashboardColors.lightBlue.gradient, color: '#1976D2' }}>
            <CardContent sx={{ p: 2 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography variant="h5" sx={{ fontWeight: 'bold' }}>{todayAttendance ? (todayAttendance.logoutTime ? 'Out' : 'In') : 'Not In'}</Typography>
                  <Typography variant="body1">Today's Status</Typography>
                  <Typography variant="body2" sx={{ opacity: 0.8 }}>{todayAttendance?.loginTime || 'No record'}</Typography>
                </Box>
                <CalendarToday sx={{ fontSize: 24 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Charts Section */}
      <Grid container spacing={2} sx={{ mb: 4 }}>
        <Grid item xs={12} md={4}>
          <Card sx={{ height: 240, background: dashboardColors.blue.gradient, color: 'white' }}>
            <CardContent sx={{ p: 2, height: '100%', display: 'flex', flexDirection: 'column' }}>
              <Typography variant="h6" gutterBottom>Leave Balance</Typography>
              <Box sx={{ position: 'relative', flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                <ResponsiveContainer width="100%" height={140}>
                  <PieChart>
                    <Pie data={leaveChartData} cx="50%" cy="50%" innerRadius={30} outerRadius={50} dataKey="value">
                      {leaveChartData.map((_, index) => <Cell key={`cell-${index}`} fill={COLORS[index]} />)}
                    </Pie>
                  </PieChart>
                </ResponsiveContainer>
                <Box sx={{ position: 'absolute', textAlign: 'center' }}>
                  <Typography variant="h5" sx={{ fontWeight: 'bold' }}>{totalLeaveBalance}</Typography>
                  <Typography variant="caption">Days Left</Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={4}>
          <Card sx={{ height: 240, background: dashboardColors.green.gradient, color: 'white' }}>
            <CardContent sx={{ p: 2, height: '100%', display: 'flex', flexDirection: 'column' }}>
              <Typography variant="h6" gutterBottom>Attendance This Month</Typography>
              <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column', justifyContent: 'center' }}>
                <Box sx={{ mb: 2 }}>
                  <Typography variant="h4" sx={{ fontWeight: 'bold', mb: 1 }}>{attendanceRate.toFixed(0)}%</Typography>
                  <Typography variant="body2" sx={{ opacity: 0.9 }}>{presentDays} of {totalWorkingDays} days</Typography>
                </Box>
                <LinearProgress variant="determinate" value={attendanceRate} sx={{ height: 8, borderRadius: 4, bgcolor: 'rgba(255,255,255,0.3)', '& .MuiLinearProgress-bar': { bgcolor: '#81C784', borderRadius: 4 } }} />
                <Box sx={{ display: 'flex', gap: 1, mt: 2 }}>
                  <Button variant="contained" startIcon={<Login fontSize="small" />} onClick={handleCheckIn} disabled={loading || Boolean(todayAttendance && !todayAttendance.logoutTime)} size="small" sx={{ bgcolor: 'rgba(255,255,255,0.2)', '&:hover': { bgcolor: 'rgba(255,255,255,0.3)' } }}>Check In</Button>
                  <Button variant="contained" startIcon={<Logout fontSize="small" />} onClick={handleCheckOut} disabled={loading || !todayAttendance || Boolean(todayAttendance?.logoutTime)} size="small" sx={{ bgcolor: 'rgba(255,255,255,0.2)', '&:hover': { bgcolor: 'rgba(255,255,255,0.3)' } }}>Check Out</Button>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={4}>
          <Card sx={{ height: 240, background: dashboardColors.blueGreen.gradient, color: 'white' }}>
            <CardContent sx={{ p: 2, height: '100%', display: 'flex', flexDirection: 'column' }}>
              <Typography variant="h6" gutterBottom>Working Hours</Typography>
              <Box sx={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', position: 'relative' }}>
                <CircularProgress variant="determinate" value={(totalWorkingHours / targetHours) * 100} size={80} thickness={6} sx={{ color: '#4CAF50', '& .MuiCircularProgress-circle': { strokeLinecap: 'round' } }} />
                <Box sx={{ position: 'absolute', textAlign: 'center' }}>
                  <Typography variant="h5" sx={{ fontWeight: 'bold' }}>{totalWorkingHours.toFixed(0)}</Typography>
                  <Typography variant="caption">/ {targetHours}h</Typography>
                </Box>
              </Box>
              <Typography variant="body2" sx={{ textAlign: 'center', opacity: 0.9 }}>This Month Progress</Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Recent Attendance */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>Recent Attendance</Typography>
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Date</TableCell><TableCell>Login</TableCell><TableCell>Logout</TableCell><TableCell>Hours</TableCell><TableCell>Status</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {attendance.slice(0, 10).map((att) => (
                  <TableRow key={att.attendanceId}>
                    <TableCell>{new Date(att.date).toLocaleDateString()}</TableCell>
                    <TableCell>{att.loginTime || '-'}</TableCell>
                    <TableCell>{att.logoutTime || '-'}</TableCell>
                    <TableCell>{att.workingHours?.toFixed(1) || '0'}</TableCell>
                    <TableCell><Chip label={att.status} color={getStatusColor(att.status) as any} size="small" /></TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </CardContent>
      </Card>
    </Container>
  );
};

export default EmployeeDashboard;