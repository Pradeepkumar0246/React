import React, { useEffect, useState, type MouseEvent } from 'react';
import { Box, Typography, Container, Card, CardContent, Button, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Chip } from '@mui/material';
import { Receipt, BeachAccess, Description, AccessTime, Login, Logout } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { attendanceService } from '../services/attendanceService';
import type { Attendance } from '../types/attendance';

interface ModuleCard {
  title: string;
  description: string;
  icon: React.ReactElement;
  path: string;
  gradient: string;
}

const EmployeeDashboard: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [attendance, setAttendance] = useState<Attendance[]>([]);
  const [loading, setLoading] = useState(false);
  const [todayAttendance, setTodayAttendance] = useState<Attendance | null>(null);

  const modules: ModuleCard[] = [
    {
      title: 'Payroll',
      description: 'View and download your payslips',
      icon: <Receipt sx={{ fontSize: 40 }} />,
      path: '/employee/payroll',
      gradient: 'linear-gradient(135deg, #319795 0%, #2c7a7b 100%)',
    },
    {
      title: 'Leave Requests',
      description: 'Request leave and check your balance',
      icon: <BeachAccess sx={{ fontSize: 40 }} />,
      path: '/employee/leave-requests',
      gradient: 'linear-gradient(135deg, #ed8936 0%, #dd6b20 100%)',
    },
    {
      title: 'Documents',
      description: 'Access your documents and upload files',
      icon: <Description sx={{ fontSize: 40 }} />,
      path: '/employee/documents',
      gradient: 'linear-gradient(135deg, #805ad5 0%, #6b46c1 100%)',
    },
  ];

  useEffect(() => { if (user?.employeeId) loadAttendance(); }, [user?.employeeId]);

  const loadAttendance = async () => {
    try {
      setLoading(true);
      const data = await attendanceService.getByEmployee(user?.employeeId || '');
      setAttendance(data || []);
      const today = new Date().toISOString().split('T')[0];
      const todayRecord = data?.find(a => a.date.split('T')[0] === today);
      setTodayAttendance(todayRecord || null);
    } catch (err) { console.error('Load error:', err); } finally { setLoading(false); }
  };

  const handleCheckIn = async (event: MouseEvent<HTMLButtonElement>) => {
    event.preventDefault();
    try {
      await attendanceService.checkIn(user?.employeeId || '');
      await loadAttendance();
    } catch (err) { console.error('Check-in failed:', err); }
  };

  const handleCheckOut = async (event: MouseEvent<HTMLButtonElement>) => {
    event.preventDefault();
    try {
      await attendanceService.checkOut(user?.employeeId || '');
      await loadAttendance();
    } catch (err) { console.error('Check-out failed:', err); }
  };

  const getStatusColor = (status: string): 'success' | 'error' | 'warning' | 'info' | 'default' => {
    switch (status) {
      case 'Present': return 'success';
      case 'Absent': return 'error';
      case 'Leave': return 'warning';
      case 'Holiday': return 'info';
      default: return 'default';
    }
  };

  return (
    <Container disableGutters sx={{ py: 4, px: 3 }}>
      <Typography variant="h4" gutterBottom>Welcome, {user?.name}!</Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
        Access your payroll information, manage leave requests, attendance, and documents.
      </Typography>
      
      <Box sx={{ maxWidth: '1200px', margin: '0 auto', display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: 3, mb: 4 }}>
        {modules.map((module) => (
          <Card key={module.title} sx={{ height: '100%', cursor: 'pointer', transition: 'all 0.3s ease', background: module.gradient, color: 'white', '&:hover': { transform: 'translateY(-4px)', boxShadow: 6 } }} onClick={() => navigate(module.path)}>
            <CardContent sx={{ p: 3, textAlign: 'center' }}>
              <Box sx={{ mb: 2 }}>{module.icon}</Box>
              <Typography variant="h6" gutterBottom>{module.title}</Typography>
              <Typography variant="body2" sx={{ opacity: 0.9 }}>{module.description}</Typography>
            </CardContent>
          </Card>
        ))}
        
        <Card sx={{ height: '100%', background: 'linear-gradient(135deg, #38a169 0%, #2f855a 100%)', color: 'white' }}>
          <CardContent sx={{ p: 3, textAlign: 'center' }}>
            <Box sx={{ mb: 2 }}><AccessTime sx={{ fontSize: 40 }} /></Box>
            <Typography variant="h6" gutterBottom>Attendance</Typography>
            <Typography variant="body2" sx={{ opacity: 0.9, mb: 2 }}>Check-in and track your attendance</Typography>
            <Box sx={{ display: 'flex', gap: 1, justifyContent: 'center' }}>
              <Button variant="contained" size="small" startIcon={<Login />} onClick={handleCheckIn} disabled={loading || Boolean(todayAttendance && !todayAttendance.logoutTime)} sx={{ bgcolor: 'rgba(255,255,255,0.2)', '&:hover': { bgcolor: 'rgba(255,255,255,0.3)' } }}>Check In</Button>
              <Button variant="contained" size="small" startIcon={<Logout />} onClick={handleCheckOut} disabled={loading || !todayAttendance || Boolean(todayAttendance?.logoutTime)} sx={{ bgcolor: 'rgba(255,255,255,0.2)', '&:hover': { bgcolor: 'rgba(255,255,255,0.3)' } }}>Check Out</Button>
            </Box>
          </CardContent>
        </Card>
      </Box>

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
                    <TableCell><Chip label={att.status} color={getStatusColor(att.status)} size="small" /></TableCell>
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