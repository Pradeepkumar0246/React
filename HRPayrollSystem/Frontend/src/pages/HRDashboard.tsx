import React, { useEffect, useState } from "react";
import { Box, Typography, Container, Card, CardContent, LinearProgress, Chip, Button, Paper } from "@mui/material";
import { Grid } from '@mui/material';
import { People, Business, BeachAccess, AccountBalance, Add } from "@mui/icons-material";
import { useNavigate } from "react-router-dom";
import { PieChart, Pie, Cell, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { employeeService } from '../services/employeeService';
import { leaveRequestService } from '../services/leaveRequestService';
import { payrollService } from '../services/payrollService';
import { attendanceService } from '../services/attendanceService';
import { dashboardColors } from '../theme/theme';
import type { Employee } from '../types/employee';
import type { LeaveRequest } from '../types/leaveRequest';
import type { Payroll } from '../types/payroll';

const HRDashboard: React.FC = () => {
  const navigate = useNavigate();
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [leaveRequests, setLeaveRequests] = useState<LeaveRequest[]>([]);
  const [payrolls, setPayrolls] = useState<Payroll[]>([]);
  const [attendanceData, setAttendanceData] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => { loadDashboardData(); }, []);

  const loadDashboardData = async () => {
    try {
      setLoading(true);
      const [emps, leaves, payrollData, attendance] = await Promise.all([
        employeeService.getAllEmployees(),
        leaveRequestService.getAll(),
        payrollService.getAll(),
        attendanceService.getAll().catch(() => [])
      ]);
      setEmployees(emps || []);
      setLeaveRequests(leaves || []);
      setPayrolls(payrollData || []);
      setAttendanceData(attendance || []);
    } catch (err) {
      console.error('Dashboard load error:', err);
    } finally {
      setLoading(false);
    }
  };

  const totalEmployees = employees.length;
  const activeEmployees = employees.filter(e => e.status === 'Active').length;
  const pendingLeaves = leaveRequests.filter(l => l.status === 'Pending').length;
  const currentMonth = new Date().toISOString().slice(0, 7);
  const currentMonthPayroll = payrolls.find(p => p.payrollMonth?.startsWith(currentMonth));
  
  const departmentCounts = employees.reduce((acc, emp) => {
    const dept = emp.departmentName || 'Unknown';
    acc[dept] = (acc[dept] || 0) + 1;
    return acc;
  }, {} as Record<string, number>);

  const topDepartments = Object.entries(departmentCounts).sort(([,a], [,b]) => b - a).slice(0, 3);

  const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8', '#82CA9D'];
  const departmentChartData = Object.entries(departmentCounts).map(([name, value]) => ({ name, value }));
  
  const attendanceTrends = attendanceData.reduce((acc: any, att: any) => {
    const month = new Date(att.date).toLocaleDateString('en-US', { month: 'short' });
    if (!acc[month]) acc[month] = { month, Present: 0, Absent: 0, Leave: 0 };
    acc[month][att.status] = (acc[month][att.status] || 0) + 1;
    return acc;
  }, {});
  const attendanceChartData = Object.values(attendanceTrends).slice(-6);
  
  const salaryRanges = employees.reduce((acc: any, emp: any) => {
    const salary = emp.basicSalary || 0;
    const range = salary < 30000 ? '<30k' : salary < 50000 ? '30k-50k' : salary < 70000 ? '50k-70k' : '70k+';
    acc[range] = (acc[range] || 0) + 1;
    return acc;
  }, {});
  const salaryChartData = Object.entries(salaryRanges).map(([range, count]) => ({ range, count }));

  if (loading) return (
    <Container sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>HR Dashboard</Typography>
      <LinearProgress sx={{ mb: 4 }} />
    </Container>
  );

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>HR Dashboard</Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>Overview of your organization's key metrics and quick actions</Typography>

      {/* Key Metrics */}
      <Grid container spacing={2} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} lg={3}>
          <Card sx={{ cursor: 'pointer', background: dashboardColors.blue.gradient, color: 'white', '&:hover': { boxShadow: 4 } }} onClick={() => navigate('/employees')}>
            <CardContent sx={{ p: 2 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography variant="h5" sx={{ fontWeight: 'bold' }}>{totalEmployees}</Typography>
                  <Typography variant="body1">Total Employees</Typography>
                  <Typography variant="body2" sx={{ opacity: 0.9 }}>{activeEmployees} active</Typography>
                </Box>
                <People sx={{ fontSize: 24 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} lg={3}>
          <Card sx={{ cursor: 'pointer', background: dashboardColors.green.gradient, color: 'white', '&:hover': { boxShadow: 4 } }} onClick={() => navigate('/leave-requests')}>
            <CardContent sx={{ p: 2 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography variant="h5" sx={{ fontWeight: 'bold' }}>{pendingLeaves}</Typography>
                  <Typography variant="body1">Pending Leaves</Typography>
                  <Typography variant="body2" sx={{ opacity: 0.9 }}>Require approval</Typography>
                </Box>
                <BeachAccess sx={{ fontSize: 24 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} lg={3}>
          <Card sx={{ cursor: 'pointer', background: dashboardColors.blueGreen.gradient, color: 'white', '&:hover': { boxShadow: 4 } }} onClick={() => navigate('/payroll')}>
            <CardContent sx={{ p: 2 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography variant="h5" sx={{ fontWeight: 'bold' }}>{currentMonthPayroll ? 'Processed' : 'Pending'}</Typography>
                  <Typography variant="body1">Payroll Status</Typography>
                  <Typography variant="body2" sx={{ opacity: 0.9 }}>{new Date().toLocaleDateString('en-US', { month: 'long', year: 'numeric' })}</Typography>
                </Box>
                <AccountBalance sx={{ fontSize: 24 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} lg={3}>
          <Card sx={{ cursor: 'pointer', background: dashboardColors.lightBlue.gradient, color: '#1976D2', '&:hover': { boxShadow: 4 } }} onClick={() => navigate('/departments')}>
            <CardContent sx={{ p: 2 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography variant="h5" sx={{ fontWeight: 'bold' }}>{Object.keys(departmentCounts).length}</Typography>
                  <Typography variant="body1">Departments</Typography>
                  <Typography variant="body2" sx={{ opacity: 0.8 }}>Active departments</Typography>
                </Box>
                <Business sx={{ fontSize: 24 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Charts Section */}
      <Grid container spacing={2} sx={{ mb: 4 }}>
        <Grid item xs={12} md={6} lg={4}>
          <Card sx={{ height: 280, background: dashboardColors.blue.gradient, color: 'white' }}>
            <CardContent sx={{ p: 2, height: '100%', display: 'flex', flexDirection: 'column' }}>
              <Typography variant="h6" gutterBottom>Department Headcount</Typography>
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie data={departmentChartData} cx="50%" cy="50%" outerRadius={50} dataKey="value">
                    {departmentChartData.map((_, index) => <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />)}
                  </Pie>
                  <Tooltip formatter={(value, name) => [`${value} employees`, name]} />
                </PieChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6} lg={4}>
          <Card sx={{ height: 280, background: dashboardColors.green.gradient, color: 'white' }}>
            <CardContent sx={{ p: 2, height: '100%', display: 'flex', flexDirection: 'column' }}>
              <Typography variant="h6" gutterBottom>Attendance Trends</Typography>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={attendanceChartData}>
                  <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.3)" />
                  <XAxis dataKey="month" tick={{ fill: 'white', fontSize: 12 }} />
                  <YAxis tick={{ fill: 'white', fontSize: 12 }} />
                  <Tooltip contentStyle={{ backgroundColor: 'rgba(0,0,0,0.8)', border: 'none', borderRadius: '8px' }} />
                  <Bar dataKey="Present" fill="#81C784" />
                  <Bar dataKey="Absent" fill="#FF8042" />
                  <Bar dataKey="Leave" fill="#64B5F6" />
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6} lg={4}>
          <Card sx={{ height: 280, background: dashboardColors.blueGreen.gradient, color: 'white' }}>
            <CardContent sx={{ p: 2, height: '100%', display: 'flex', flexDirection: 'column' }}>
              <Typography variant="h6" gutterBottom>Salary Distribution</Typography>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={salaryChartData}>
                  <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.3)" />
                  <XAxis dataKey="range" tick={{ fill: 'white', fontSize: 12 }} />
                  <YAxis tick={{ fill: 'white', fontSize: 12 }} />
                  <Tooltip contentStyle={{ backgroundColor: 'rgba(0,0,0,0.8)', border: 'none', borderRadius: '8px' }} />
                  <Bar dataKey="count" fill="#4CAF50" />
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Dashboard Sections */}
      <Grid container spacing={2}>
        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 2, height: 240, background: dashboardColors.lightBlue.gradient }}>
            <Typography variant="h6" gutterBottom sx={{ mb: 3, color: '#1976D2' }}>Quick Actions</Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/employees')} fullWidth sx={{ bgcolor: dashboardColors.blue.main }}>Add Employee</Button>
              <Button variant="contained" startIcon={<AccountBalance />} onClick={() => navigate('/payroll')} fullWidth sx={{ bgcolor: dashboardColors.green.main }}>Generate Payroll</Button>
              <Button variant="contained" startIcon={<BeachAccess />} onClick={() => navigate('/leave-requests')} fullWidth sx={{ background: dashboardColors.blueGreen.gradient }}>
                Review Leaves {pendingLeaves > 0 && <Chip label={pendingLeaves} size="small" sx={{ ml: 1, bgcolor: 'white', color: '#333' }} />}
              </Button>
            </Box>
          </Paper>
        </Grid>

        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 2, height: 240, background: dashboardColors.lightGreen.gradient }}>
            <Typography variant="h6" gutterBottom sx={{ mb: 3, color: '#388E3C' }}>Top Departments</Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              {topDepartments.map(([dept, count]) => (
                <Box key={dept} sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', p: 2, borderRadius: 2, bgcolor: 'rgba(255,255,255,0.8)' }}>
                  <Typography variant="body1" sx={{ fontWeight: 600, color: '#333' }}>{dept}</Typography>
                  <Chip label={`${count}`} sx={{ bgcolor: dashboardColors.green.main, color: 'white', fontWeight: 'bold' }} />
                </Box>
              ))}
            </Box>
          </Paper>
        </Grid>

        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 2, height: 240, background: 'linear-gradient(135deg, #E3F2FD 0%, #E8F5E8 100%)' }}>
            <Typography variant="h6" gutterBottom sx={{ mb: 3, color: '#1976D2' }}>System Status</Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <Box sx={{ p: 2, borderRadius: 2, bgcolor: 'rgba(255,255,255,0.8)' }}>
                <Typography variant="body2" color="#333">Active Employees</Typography>
                <Typography variant="h5" sx={{ fontWeight: 'bold', color: dashboardColors.green.dark }}>{activeEmployees}</Typography>
              </Box>
              <Box sx={{ p: 2, borderRadius: 2, bgcolor: 'rgba(255,255,255,0.8)' }}>
                <Typography variant="body2" color="#333">Payroll Status</Typography>
                <Typography variant="h6" sx={{ fontWeight: 'bold', color: currentMonthPayroll ? dashboardColors.green.dark : '#d32f2f' }}>
                  {currentMonthPayroll ? 'Processed' : 'Pending'}
                </Typography>
              </Box>
            </Box>
          </Paper>
        </Grid>
      </Grid>
    </Container>
  );
};

export default HRDashboard;