import React, { useEffect, useState } from 'react';
import { Box, Typography, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, IconButton, Tooltip, Dialog, DialogTitle, DialogContent, DialogActions, Button } from '@mui/material';
import { Visibility, Download } from '@mui/icons-material';
import { useAuth } from '../../auth/AuthContext';
import { payslipService } from '../../services/payslipService';

type Payslip = {
  payslipId: number;
  employeeId: string;
  employeeName: string;
  payrollId: number;
  payrollMonth: string;
  filePath: string;
  fileName: string;
  generatedDate: string;
  netPay: number;
};

const EmployeePayroll: React.FC = () => {
  const { user } = useAuth();
  const [payslips, setPayslips] = useState<Payslip[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedPayslip, setSelectedPayslip] = useState<Payslip | null>(null);
  const [openDialog, setOpenDialog] = useState(false);

  useEffect(() => { if (user?.employeeId) loadPayslips(); }, [user?.employeeId]);

  const loadPayslips = async () => {
    try {
      setLoading(true);
      const data = await payslipService.getByEmployee(user?.employeeId || '');
      setPayslips(data || []);
    } catch (err) { console.error('Load error:', err); } finally { setLoading(false); }
  };

  const handleView = (payslip: Payslip) => {
    setSelectedPayslip(payslip);
    setOpenDialog(true);
  };

  const handleDownload = async (payslip: Payslip) => {
    try {
      const blob = await payslipService.download(payslip.payslipId);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = payslip.fileName || `payslip-${payslip.payrollMonth}.pdf`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (err) { console.error('Download failed:', err); }
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>My Payroll</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>View and download your payslips</Typography>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Month</TableCell><TableCell>Net Pay</TableCell><TableCell>Generated Date</TableCell><TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {payslips.map((payslip) => (
              <TableRow key={payslip.payslipId}>
                <TableCell>{new Date(payslip.payrollMonth).toLocaleDateString('en-US', { year: 'numeric', month: 'long' })}</TableCell>
                <TableCell>₹{payslip.netPay?.toLocaleString()}</TableCell>
                <TableCell>{new Date(payslip.generatedDate).toLocaleDateString()}</TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    <Tooltip title="View Details"><IconButton size="small" onClick={() => handleView(payslip)}><Visibility /></IconButton></Tooltip>
                    <Tooltip title="Download"><IconButton size="small" onClick={() => handleDownload(payslip)}><Download /></IconButton></Tooltip>
                  </Box>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Payslip Details</DialogTitle>
        <DialogContent>
          {selectedPayslip && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                <Typography variant="body2" color="text.secondary">Employee:</Typography>
                <Typography variant="body2">{selectedPayslip.employeeName}</Typography>
              </Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                <Typography variant="body2" color="text.secondary">Month:</Typography>
                <Typography variant="body2">{new Date(selectedPayslip.payrollMonth).toLocaleDateString('en-US', { year: 'numeric', month: 'long' })}</Typography>
              </Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                <Typography variant="body2" color="text.secondary">Net Pay:</Typography>
                <Typography variant="body2" sx={{ fontWeight: 'bold' }}>₹{selectedPayslip.netPay?.toLocaleString()}</Typography>
              </Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                <Typography variant="body2" color="text.secondary">Generated:</Typography>
                <Typography variant="body2">{new Date(selectedPayslip.generatedDate).toLocaleDateString()}</Typography>
              </Box>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDialog(false)}>Close</Button>
          {selectedPayslip && <Button variant="contained" onClick={() => handleDownload(selectedPayslip)}>Download</Button>}
        </DialogActions>
      </Dialog>

      {loading && <Typography sx={{ mt: 2 }}>Loading...</Typography>}
    </Box>
  );
};

export default EmployeePayroll;