import React, { useEffect, useState } from 'react';
import { Box, Button, Card, CardContent, Chip, FormControl, InputLabel, MenuItem, Paper, Select, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from '@mui/material';
import { Download, Receipt } from '@mui/icons-material';
import { useAuth } from '../../auth/AuthContext';
import { payslipService } from '../../services/payslipService';
import type { Payslip } from '../../types/payroll';

const EmployeePayslips: React.FC = () => {
  const { user } = useAuth();
  const [payslips, setPayslips] = useState<Payslip[]>([]);
  const [loading, setLoading] = useState(false);
  const [yearFilter, setYearFilter] = useState<string>(new Date().getFullYear().toString());
  const [monthFilter, setMonthFilter] = useState<string>('');

  useEffect(() => {
    if (user?.employeeId) loadPayslips();
  }, [user?.employeeId]);

  const loadPayslips = async () => {
    try {
      setLoading(true);
      const data = await payslipService.getByEmployee(user?.employeeId || '');
      setPayslips(data || []);
    } catch (err) {
      console.error('Load error:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDownload = async (payslipId: number, fileName: string) => {
    try {
      const blob = await payslipService.download(payslipId);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName;
      a.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      const error = err as { response?: { data?: { message?: string } }; message?: string };
      console.error('Download failed:', err);
      alert(`Download failed: ${error.response?.data?.message || error.message}`);
    }
  };

  const filteredPayslips = payslips.filter(payslip => {
    const payrollDate = new Date(payslip.payrollMonth);
    const yearMatch = !yearFilter || payrollDate.getFullYear().toString() === yearFilter;
    const monthMatch = !monthFilter || (payrollDate.getMonth() + 1).toString().padStart(2, '0') === monthFilter;
    return yearMatch && monthMatch;
  });

  const years = [...new Set(payslips.map(p => new Date(p.payrollMonth).getFullYear()))].sort((a, b) => b - a);
  const months = [
    { value: '01', label: 'January' }, { value: '02', label: 'February' }, { value: '03', label: 'March' },
    { value: '04', label: 'April' }, { value: '05', label: 'May' }, { value: '06', label: 'June' },
    { value: '07', label: 'July' }, { value: '08', label: 'August' }, { value: '09', label: 'September' },
    { value: '10', label: 'October' }, { value: '11', label: 'November' }, { value: '12', label: 'December' }
  ];

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>My Payslips</Typography>
      
      <Paper sx={{ p: 2, mb: 3 }}>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <FormControl sx={{ minWidth: 120 }}>
            <InputLabel>Year</InputLabel>
            <Select value={yearFilter} label="Year" onChange={(e) => setYearFilter(e.target.value)}>
              <MenuItem value="">All</MenuItem>
              {years.map(year => <MenuItem key={year} value={year.toString()}>{year}</MenuItem>)}
            </Select>
          </FormControl>
          <FormControl sx={{ minWidth: 120 }}>
            <InputLabel>Month</InputLabel>
            <Select value={monthFilter} label="Month" onChange={(e) => setMonthFilter(e.target.value)}>
              <MenuItem value="">All</MenuItem>
              {months.map(month => <MenuItem key={month.value} value={month.value}>{month.label}</MenuItem>)}
            </Select>
          </FormControl>
        </Box>
      </Paper>

      {loading ? (
        <Typography>Loading...</Typography>
      ) : filteredPayslips.length === 0 ? (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 4 }}>
            <Receipt sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
            <Typography variant="h6" color="text.secondary">No payslips found</Typography>
          </CardContent>
        </Card>
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>File Name</TableCell>
                <TableCell>Generated Date</TableCell>
                <TableCell>Month/Year</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredPayslips.map((payslip) => (
                <TableRow key={payslip.payslipId}>
                  <TableCell>{payslip.fileName}</TableCell>
                  <TableCell>{new Date(payslip.generatedDate).toLocaleDateString()}</TableCell>
                  <TableCell>
                    <Chip 
                      label={new Date(payslip.payrollMonth).toLocaleDateString('en-US', { year: 'numeric', month: 'long' })} 
                      color="primary" 
                      size="small" 
                    />
                  </TableCell>
                  <TableCell>
                    <Button
                      size="small"
                      startIcon={<Download />}
                      onClick={() => handleDownload(payslip.payslipId, payslip.fileName)}
                    >
                      Download
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
};

export default EmployeePayslips;