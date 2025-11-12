import React, { useEffect, useMemo, useState } from 'react';
import { Avatar, Box, Button, Container, Dialog, DialogActions, DialogContent, DialogTitle, FormControl, IconButton, InputLabel, MenuItem, Paper, Select, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Tooltip, Typography } from '@mui/material';
import { Add, Edit, AttachMoney, Search } from '@mui/icons-material';
import { useAuth } from '../auth/AuthContext';
import { salaryStructureService } from '../services/salaryStructureService';
import { employeeService } from '../services/employeeService';
import type { SalaryStructure, SalaryStructureFilters } from '../types/salaryStructure';
import type { Employee } from '../types/employee';

type FormState = { employeeId: string; basicSalary: string; hra: string; allowances: string; deductions: string; pf: string; tax: string };

const SalaryStructurePage: React.FC = () => {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin' || user?.role === 'HR Manager';
  const [salaryStructures, setSalaryStructures] = useState<SalaryStructure[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(false);
  const [filters, setFilters] = useState<SalaryStructureFilters>({ search: '', employeeId: '' });
  const [openForm, setOpenForm] = useState(false);
  const [editStructure, setEditStructure] = useState<SalaryStructure | null>(null);
  const [form, setForm] = useState<FormState>({ employeeId: '', basicSalary: '', hra: '', allowances: '', deductions: '', pf: '', tax: '' });

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [structuresData, employeesData] = await Promise.all([
        isAdmin ? salaryStructureService.getAll() : [await salaryStructureService.getByEmployee(user?.employeeId || '')].filter(Boolean),
        employeeService.getAllEmployees()
      ]);
      setSalaryStructures(structuresData ?? []); setEmployees(employeesData ?? []);
    } catch (err) { console.error('Load error:', err); } finally { setLoading(false); }
  };

  const filtered = useMemo(() => {
    const s = (filters.search || '').toLowerCase();
    return salaryStructures.filter((ss) => {
      const matchSearch = ss.employeeName?.toLowerCase().includes(s) || ss.employeeId?.toLowerCase().includes(s);
      return matchSearch && (!filters.employeeId || ss.employeeId === filters.employeeId);
    });
  }, [salaryStructures, filters]);

  const calculateNetSalary = (basic: number, hra: number, allowances: number, deductions: number, pf: number, tax: number) => {
    return basic + hra + allowances - deductions - pf - tax;
  };

  const handleOpenForm = (structure?: SalaryStructure) => {
    setEditStructure(structure ?? null);
    setForm(structure ? { 
      employeeId: structure.employeeId, 
      basicSalary: structure.basicSalary.toString(), 
      hra: structure.hra.toString(), 
      allowances: structure.allowances.toString(), 
      deductions: structure.deductions.toString(), 
      pf: structure.pf.toString(), 
      tax: structure.tax.toString() 
    } : { employeeId: '', basicSalary: '', hra: '', allowances: '', deductions: '', pf: '', tax: '' });
    setOpenForm(true);
  };

  const handleSubmit = async () => {
    try {
      const data = {
        employeeId: form.employeeId,
        basicSalary: parseFloat(form.basicSalary) || 0,
        hra: parseFloat(form.hra) || 0,
        allowances: parseFloat(form.allowances) || 0,
        deductions: parseFloat(form.deductions) || 0,
        pf: parseFloat(form.pf) || 0,
        tax: parseFloat(form.tax) || 0
      };
      
      if (editStructure) {
        await salaryStructureService.update(editStructure.salaryStructureId, { ...data, salaryStructureId: editStructure.salaryStructureId, netSalary: calculateNetSalary(data.basicSalary, data.hra, data.allowances, data.deductions, data.pf, data.tax) });
      } else {
        await salaryStructureService.create(data);
      }
      await loadData(); setOpenForm(false);
    } catch (err) { console.error('Save failed:', err); }
  };

  const currentNetSalary = useMemo(() => {
    const basic = parseFloat(form.basicSalary) || 0;
    const hra = parseFloat(form.hra) || 0;
    const allowances = parseFloat(form.allowances) || 0;
    const deductions = parseFloat(form.deductions) || 0;
    const pf = parseFloat(form.pf) || 0;
    const tax = parseFloat(form.tax) || 0;
    return calculateNetSalary(basic, hra, allowances, deductions, pf, tax);
  }, [form]);

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>Salary Structure</Typography>

      <Paper sx={{ p: 2, mb: 3 }}>
        <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 2, alignItems: 'center' }}>
          <TextField placeholder="Search by name or ID" value={filters.search} onChange={(e) => setFilters(s => ({ ...s, search: e.target.value }))} InputProps={{ startAdornment: <Search sx={{ mr: 1, color: 'text.secondary' }} /> }} />
          <FormControl><InputLabel>Employee</InputLabel><Select value={filters.employeeId} label="Employee" onChange={(e) => setFilters(s => ({ ...s, employeeId: e.target.value }))}><MenuItem value="">All</MenuItem>{employees.map((e) => <MenuItem key={e.employeeId} value={e.employeeId}>{e.fullName}</MenuItem>)}</Select></FormControl>
        </Box>
      </Paper>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Box>
          {isAdmin && <Button variant="contained" startIcon={<Add />} onClick={() => handleOpenForm()}>Add Salary Structure</Button>}
        </Box>
        <Typography variant="body2" color="text.secondary">{filtered.length} records found</Typography>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Employee</TableCell><TableCell>Basic Salary</TableCell><TableCell>HRA</TableCell><TableCell>Allowances</TableCell><TableCell>Gross Salary</TableCell><TableCell>Deductions</TableCell><TableCell>PF</TableCell><TableCell>Tax</TableCell><TableCell>Net Salary</TableCell><TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filtered.map((structure) => (
              <TableRow key={structure.salaryStructureId}>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Avatar sx={{ bgcolor: 'primary.main' }}><AttachMoney /></Avatar>
                    <Box><Typography variant="subtitle2">{structure.employeeName}</Typography><Typography variant="body2" color="text.secondary">{structure.employeeId}</Typography></Box>
                  </Box>
                </TableCell>
                <TableCell>₹{structure.basicSalary.toLocaleString()}</TableCell>
                <TableCell>₹{structure.hra.toLocaleString()}</TableCell>
                <TableCell>₹{structure.allowances.toLocaleString()}</TableCell>
                <TableCell>₹{structure.grossSalary.toLocaleString()}</TableCell>
                <TableCell>₹{structure.deductions.toLocaleString()}</TableCell>
                <TableCell>₹{structure.pf.toLocaleString()}</TableCell>
                <TableCell>₹{structure.tax.toLocaleString()}</TableCell>
                <TableCell><Typography variant="subtitle2" color="primary">₹{structure.netSalary.toLocaleString()}</Typography></TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    {isAdmin && <Tooltip title="Edit"><IconButton size="small" onClick={() => handleOpenForm(structure)}><Edit /></IconButton></Tooltip>}
                  </Box>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={openForm} onClose={() => setOpenForm(false)} maxWidth="md" fullWidth>
        <DialogTitle>{editStructure ? 'Edit Salary Structure' : 'Add Salary Structure'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: 2, mt: 1 }}>
            <FormControl sx={{ gridColumn: '1 / -1' }}><InputLabel>Employee</InputLabel><Select value={form.employeeId} label="Employee" onChange={(e) => setForm(s => ({ ...s, employeeId: e.target.value }))} disabled={!!editStructure}>{employees.map((e) => <MenuItem key={e.employeeId} value={e.employeeId}>{e.fullName}</MenuItem>)}</Select></FormControl>
            <TextField type="number" label="Basic Salary" value={form.basicSalary} onChange={(e) => setForm(s => ({ ...s, basicSalary: e.target.value }))} />
            <TextField type="number" label="HRA" value={form.hra} onChange={(e) => setForm(s => ({ ...s, hra: e.target.value }))} />
            <TextField type="number" label="Allowances" value={form.allowances} onChange={(e) => setForm(s => ({ ...s, allowances: e.target.value }))} />
            <TextField type="number" label="Deductions" value={form.deductions} onChange={(e) => setForm(s => ({ ...s, deductions: e.target.value }))} />
            <TextField type="number" label="PF" value={form.pf} onChange={(e) => setForm(s => ({ ...s, pf: e.target.value }))} />
            <TextField type="number" label="Tax" value={form.tax} onChange={(e) => setForm(s => ({ ...s, tax: e.target.value }))} />
            <Box sx={{ gridColumn: '1 / -1', p: 2, bgcolor: 'grey.50', borderRadius: 1 }}>
              <Typography variant="h6" color="primary">Calculated Net Salary: ₹{currentNetSalary.toLocaleString()}</Typography>
              <Typography variant="body2" color="text.secondary">Basic + HRA + Allowances - Deductions - PF - Tax</Typography>
            </Box>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenForm(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSubmit}>{editStructure ? 'Update' : 'Create'}</Button>
        </DialogActions>
      </Dialog>

      {loading && <Typography sx={{ mt: 2 }}>Loading...</Typography>}
    </Container>
  );
};

export default SalaryStructurePage;