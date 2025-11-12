import React, { useEffect, useMemo, useState } from 'react';
import { Avatar, Box, Button, Chip, Container, Dialog, DialogActions, DialogContent, DialogTitle, FormControl, IconButton, InputLabel, MenuItem, Paper, Select, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Tooltip, Typography, Collapse } from '@mui/material';
import { Add, Edit, Delete, Download, ExpandMore, ExpandLess, Payment, Search } from '@mui/icons-material';
import { useAuth } from '../auth/AuthContext';
import { payrollService } from '../services/payrollService';
import { payslipService } from '../services/payslipService';
import { employeeService } from '../services/employeeService';
import type { Payroll, Payslip, PayrollFilters } from '../types/payroll';
import type { Employee } from '../types/employee';

type FormState = { employeeId: string; payrollMonth: string; grossSalary: string; totalDeductions: string; bonus: string; paymentStatus: string };

const PayrollPage: React.FC = () => {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin' || user?.role === 'HR Manager';
  const [payrolls, setPayrolls] = useState<Payroll[]>([]);
  const [payslips, setPayslips] = useState<{ [key: number]: Payslip }>({});
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(false);
  const [filters, setFilters] = useState<PayrollFilters>({ search: '', employeeId: '', paymentStatus: '', month: '' });
  const [openForm, setOpenForm] = useState(false);
  const [openGenerate, setOpenGenerate] = useState(false);
  const [editPayroll, setEditPayroll] = useState<Payroll | null>(null);
  const [expandedRows, setExpandedRows] = useState<Set<number>>(new Set());
  const [form, setForm] = useState<FormState>({ employeeId: '', payrollMonth: '', grossSalary: '', totalDeductions: '', bonus: '', paymentStatus: 'Pending' });
  const [generateForm, setGenerateForm] = useState({ employeeId: '', month: '', bonusAmount: 0, additionalDeductions: 0, paymentStatus: 'Pending' });

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [payrollData, employeesData] = await Promise.all([
        isAdmin ? payrollService.getAll() : payrollService.getByEmployee(user?.employeeId || ''),
        employeeService.getAllEmployees()
      ]);
      setPayrolls(payrollData ?? []); setEmployees(employeesData ?? []);
    } catch (err) { console.error('Load error:', err); } finally { setLoading(false); }
  };

  const loadPayslip = async (payrollId: number) => {
    try {
      const payslip = await payslipService.getByPayroll(payrollId);
      setPayslips(prev => ({ ...prev, [payrollId]: payslip }));
    } catch (err) { console.error('Payslip load error:', err); }
  };

  const filtered = useMemo(() => {
    const s = (filters.search || '').toLowerCase();
    return payrolls.filter((p) => {
      const matchSearch = p.employeeName?.toLowerCase().includes(s) || p.employeeId?.toLowerCase().includes(s);
      return matchSearch && (!filters.employeeId || p.employeeId === filters.employeeId) && (!filters.paymentStatus || p.paymentStatus === filters.paymentStatus) && (!filters.month || p.payrollMonth.includes(filters.month));
    });
  }, [payrolls, filters]);

  const handleOpenForm = (payroll?: Payroll) => {
    setEditPayroll(payroll ?? null);
    setForm(payroll ? { employeeId: payroll.employeeId, payrollMonth: payroll.payrollMonth.split('T')[0], grossSalary: payroll.grossSalary.toString(), totalDeductions: payroll.totalDeductions.toString(), bonus: payroll.bonus.toString(), paymentStatus: payroll.paymentStatus } : { employeeId: '', payrollMonth: new Date().toISOString().split('T')[0], grossSalary: '', totalDeductions: '', bonus: '0', paymentStatus: 'Pending' });
    setOpenForm(true);
  };

  const handleSubmit = async () => {
    try {
      const data = { ...form, grossSalary: parseFloat(form.grossSalary), totalDeductions: parseFloat(form.totalDeductions), bonus: parseFloat(form.bonus), netPay: parseFloat(form.grossSalary) + parseFloat(form.bonus) - parseFloat(form.totalDeductions), paymentDate: new Date().toISOString(), paymentStatus: form.paymentStatus as 'Pending' | 'Processed' | 'Paid' };
      if (editPayroll) {
        await payrollService.update(editPayroll.payrollId, data);
      } else {
        await payrollService.generate({ employeeId: form.employeeId, month: form.payrollMonth, paymentStatus: 'Pending' });
      }
      await loadData(); setOpenForm(false);
    } catch (err) { console.error('Save failed:', err); }
  };

  const handleGenerate = async () => {
    try {
      await payrollService.generate({ ...generateForm, paymentStatus: 'Pending' });
      await loadData(); setOpenGenerate(false);
    } catch (err) {
      const error = err as { response?: { data?: { message?: string }; status?: number }; message?: string };
      console.error('Generate failed:', err);
      console.error('Error details:', error.response?.data);
      console.error('Status:', error.response?.status);
      alert(`Generate failed: ${error.response?.data?.message || error.message}`);
    }
  };

  const handleDelete = async (id: number) => {
    if (confirm('Delete this payroll?')) {
      try { await payrollService.delete(id); await loadData(); } catch (err) { console.error('Delete failed:', err); }
    }
  };

  const handleDownload = async (payslipId: number, fileName: string) => {
    try {
      const blob = await payslipService.download(payslipId);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url; a.download = fileName; a.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      const error = err as { response?: { data?: { message?: string }; status?: number }; message?: string };
      console.error('Download failed:', err);
      console.error('Error details:', error.response?.data);
      console.error('Status:', error.response?.status);
      alert(`Download failed: ${error.response?.data?.message || error.message}`);
    }
  };

  const toggleExpand = (payrollId: number) => {
    const newExpanded = new Set(expandedRows);
    if (newExpanded.has(payrollId)) {
      newExpanded.delete(payrollId);
    } else {
      newExpanded.add(payrollId);
      if (!payslips[payrollId]) loadPayslip(payrollId);
    }
    setExpandedRows(newExpanded);
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Paid': return 'success';
      case 'Processed': return 'warning';
      case 'Pending': return 'error';
      default: return 'default';
    }
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>Payroll Management</Typography>

      <Paper sx={{ p: 2, mb: 3 }}>
        <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 2, alignItems: 'center' }}>
          <TextField placeholder="Search by name or ID" value={filters.search} onChange={(e) => setFilters(s => ({ ...s, search: e.target.value }))} InputProps={{ startAdornment: <Search sx={{ mr: 1, color: 'text.secondary' }} /> }} />
          <FormControl><InputLabel>Employee</InputLabel><Select value={filters.employeeId} label="Employee" onChange={(e) => setFilters(s => ({ ...s, employeeId: e.target.value }))}><MenuItem value="">All</MenuItem>{employees.map((e) => <MenuItem key={e.employeeId} value={e.employeeId}>{e.fullName}</MenuItem>)}</Select></FormControl>
          <FormControl><InputLabel>Status</InputLabel><Select value={filters.paymentStatus} label="Status" onChange={(e) => setFilters(s => ({ ...s, paymentStatus: e.target.value }))}><MenuItem value="">All</MenuItem><MenuItem value="Pending">Pending</MenuItem><MenuItem value="Processed">Processed</MenuItem><MenuItem value="Paid">Paid</MenuItem></Select></FormControl>
          <TextField type="month" label="Month" value={filters.month} onChange={(e) => setFilters(s => ({ ...s, month: e.target.value }))} InputLabelProps={{ shrink: true }} />
        </Box>
      </Paper>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Box>
          {isAdmin && <Button variant="contained" startIcon={<Add />} onClick={() => setOpenGenerate(true)}>Generate Payroll</Button>}
        </Box>
        <Typography variant="body2" color="text.secondary">{filtered.length} records found</Typography>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Employee</TableCell><TableCell>Month</TableCell><TableCell>Gross Salary</TableCell><TableCell>Deductions</TableCell><TableCell>Bonus</TableCell><TableCell>Net Pay</TableCell><TableCell>Status</TableCell><TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filtered.map((payroll) => (
              <React.Fragment key={payroll.payrollId}>
                <TableRow>
                  <TableCell>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                      <Avatar sx={{ bgcolor: 'primary.main' }}><Payment /></Avatar>
                      <Box><Typography variant="subtitle2">{payroll.employeeName}</Typography><Typography variant="body2" color="text.secondary">{payroll.employeeId}</Typography></Box>
                    </Box>
                  </TableCell>
                  <TableCell>{new Date(payroll.payrollMonth).toLocaleDateString('en-US', { year: 'numeric', month: 'long' })}</TableCell>
                  <TableCell>₹{payroll.grossSalary.toLocaleString()}</TableCell>
                  <TableCell>₹{payroll.totalDeductions.toLocaleString()}</TableCell>
                  <TableCell>₹{payroll.bonus.toLocaleString()}</TableCell>
                  <TableCell>₹{payroll.netPay.toLocaleString()}</TableCell>
                  <TableCell><Chip label={payroll.paymentStatus} color={getStatusColor(payroll.paymentStatus) as any} size="small" /></TableCell>
                  <TableCell>
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      <Tooltip title="View Payslip"><IconButton size="small" onClick={() => toggleExpand(payroll.payrollId)}>{expandedRows.has(payroll.payrollId) ? <ExpandLess /> : <ExpandMore />}</IconButton></Tooltip>
                      {isAdmin && <Tooltip title="Edit"><IconButton size="small" onClick={() => handleOpenForm(payroll)}><Edit /></IconButton></Tooltip>}
                      {isAdmin && <Tooltip title="Delete"><IconButton size="small" onClick={() => handleDelete(payroll.payrollId)}><Delete /></IconButton></Tooltip>}
                    </Box>
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell colSpan={8} sx={{ p: 0 }}>
                    <Collapse in={expandedRows.has(payroll.payrollId)}>
                      <Box sx={{ p: 2, bgcolor: 'grey.50' }}>
                        {payslips[payroll.payrollId] ? (
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                            <Typography variant="body2">Payslip: {payslips[payroll.payrollId].fileName}</Typography>
                            <Typography variant="body2" color="text.secondary">Generated: {new Date(payslips[payroll.payrollId].generatedDate).toLocaleDateString()}</Typography>
                            <Button size="small" startIcon={<Download />} onClick={() => handleDownload(payslips[payroll.payrollId].payslipId, payslips[payroll.payrollId].fileName)}>Download</Button>
                          </Box>
                        ) : (
                          <Typography variant="body2" color="text.secondary">No payslip generated</Typography>
                        )}
                      </Box>
                    </Collapse>
                  </TableCell>
                </TableRow>
              </React.Fragment>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={openGenerate} onClose={() => setOpenGenerate(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Generate Payroll</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <FormControl><InputLabel>Employee</InputLabel><Select value={generateForm.employeeId} label="Employee" onChange={(e) => setGenerateForm(s => ({ ...s, employeeId: e.target.value }))}>{employees.map((e) => <MenuItem key={e.employeeId} value={e.employeeId}>{e.fullName}</MenuItem>)}</Select></FormControl>
            <TextField type="month" label="Payroll Month" value={generateForm.month} onChange={(e) => setGenerateForm(s => ({ ...s, month: e.target.value }))} InputLabelProps={{ shrink: true }} />
            <TextField type="number" label="Bonus Amount" value={generateForm.bonusAmount} onChange={(e) => setGenerateForm(s => ({ ...s, bonusAmount: parseFloat(e.target.value) || 0 }))} />
            <TextField type="number" label="Additional Deductions" value={generateForm.additionalDeductions} onChange={(e) => setGenerateForm(s => ({ ...s, additionalDeductions: parseFloat(e.target.value) || 0 }))} />
            <FormControl><InputLabel>Payment Status</InputLabel><Select value={generateForm.paymentStatus} label="Payment Status" onChange={(e) => setGenerateForm(s => ({ ...s, paymentStatus: e.target.value }))}><MenuItem value="Pending">Pending</MenuItem><MenuItem value="Processed">Processed</MenuItem><MenuItem value="Paid">Paid</MenuItem></Select></FormControl>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenGenerate(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleGenerate}>Generate</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={openForm} onClose={() => setOpenForm(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editPayroll ? 'Edit Payroll' : 'Add Payroll'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <FormControl><InputLabel>Employee</InputLabel><Select value={form.employeeId} label="Employee" onChange={(e) => setForm(s => ({ ...s, employeeId: e.target.value }))}>{employees.map((e) => <MenuItem key={e.employeeId} value={e.employeeId}>{e.fullName}</MenuItem>)}</Select></FormControl>
            <TextField type="month" label="Payroll Month" value={form.payrollMonth} onChange={(e) => setForm(s => ({ ...s, payrollMonth: e.target.value }))} InputLabelProps={{ shrink: true }} />
            <TextField type="number" label="Gross Salary" value={form.grossSalary} onChange={(e) => setForm(s => ({ ...s, grossSalary: e.target.value }))} />
            <TextField type="number" label="Total Deductions" value={form.totalDeductions} onChange={(e) => setForm(s => ({ ...s, totalDeductions: e.target.value }))} />
            <TextField type="number" label="Bonus" value={form.bonus} onChange={(e) => setForm(s => ({ ...s, bonus: e.target.value }))} />
            <FormControl><InputLabel>Payment Status</InputLabel><Select value={form.paymentStatus} label="Payment Status" onChange={(e) => setForm(s => ({ ...s, paymentStatus: e.target.value }))}><MenuItem value="Pending">Pending</MenuItem><MenuItem value="Processed">Processed</MenuItem><MenuItem value="Paid">Paid</MenuItem></Select></FormControl>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenForm(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSubmit}>{editPayroll ? 'Update' : 'Create'}</Button>
        </DialogActions>
      </Dialog>

      {loading && <Typography sx={{ mt: 2 }}>Loading...</Typography>}
    </Container>
  );
};

export default PayrollPage;