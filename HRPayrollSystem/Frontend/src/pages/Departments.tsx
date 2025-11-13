import React, { useEffect, useState } from 'react';
import { Avatar, Box, Button, Container, Dialog, DialogActions, DialogContent, DialogTitle, IconButton, Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Tooltip, Typography } from '@mui/material';
import { Add, Delete, Edit, Business } from '@mui/icons-material';
import { useAuth } from '../auth/AuthContext';
import { departmentService } from '../services/departmentService';
import { validateRequired, validateStringLength } from '../utils/validation';
import type { Department } from '../types/employee';

type FormState = { departmentName: string; description: string };

const Departments: React.FC = () => {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin' || user?.role === 'HR Manager';
  const [departments, setDepartments] = useState<Department[]>([]);
  const [loading, setLoading] = useState(false);
  const [openForm, setOpenForm] = useState(false);
  const [editDepartment, setEditDepartment] = useState<Department | null>(null);
  const [form, setForm] = useState<FormState>({ departmentName: '', description: '' });
  const [errors, setErrors] = useState<{[key: string]: string}>({});

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const data = await departmentService.getAll();
      setDepartments(data ?? []);
    } catch (err) { console.error('Load error:', err); } finally { setLoading(false); }
  };

  const handleOpenForm = (dept?: Department) => {
    setEditDepartment(dept ?? null);
    setForm(dept ? { departmentName: dept.departmentName, description: dept.description || '' } : { departmentName: '', description: '' });
    setOpenForm(true);
  };

  const handleSubmit = async () => {
    try {
      if (editDepartment) {
        await departmentService.update(editDepartment.departmentId, { departmentId: editDepartment.departmentId, ...form });
      } else {
        await departmentService.create(form);
      }
      await loadData(); setOpenForm(false);
    } catch (err) { console.error('Save failed:', err); }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Delete this department?')) return;
    try { await departmentService.delete(id); await loadData(); } catch (err) { console.error('Delete error:', err); }
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>Departments</Typography>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Box>
          {isAdmin && <Button variant="contained" startIcon={<Add />} onClick={() => handleOpenForm()}>Add Department</Button>}
        </Box>
        <Typography variant="body2" color="text.secondary">{departments.length} departments found</Typography>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Department</TableCell><TableCell>Description</TableCell><TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {departments.map((dept) => (
              <TableRow key={dept.departmentId}>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Avatar sx={{ bgcolor: 'primary.main' }}><Business /></Avatar>
                    <Typography variant="subtitle2">{dept.departmentName}</Typography>
                  </Box>
                </TableCell>
                <TableCell>{dept.description || 'No description'}</TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    {isAdmin && (
                      <>
                        <Tooltip title="Edit"><IconButton size="small" onClick={() => handleOpenForm(dept)}><Edit /></IconButton></Tooltip>
                        <Tooltip title="Delete"><IconButton size="small" color="error" onClick={() => handleDelete(dept.departmentId)}><Delete /></IconButton></Tooltip>
                      </>
                    )}
                  </Box>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={openForm} onClose={() => setOpenForm(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editDepartment ? 'Edit Department' : 'Add Department'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField label="Department Name" value={form.departmentName} onChange={(e) => { setForm(s => ({ ...s, departmentName: e.target.value })); if (errors.departmentName) setErrors(prev => ({...prev, departmentName: ''})); }} onBlur={() => { const error = validateRequired(form.departmentName, 'Department Name') || validateStringLength(form.departmentName, 2, 100, 'Department Name'); if (error) setErrors(prev => ({...prev, departmentName: error})); }} error={!!errors.departmentName} helperText={errors.departmentName} required />
            <TextField label="Description" multiline rows={3} value={form.description} onChange={(e) => setForm(s => ({ ...s, description: e.target.value }))} />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenForm(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSubmit}>{editDepartment ? 'Update' : 'Create'}</Button>
        </DialogActions>
      </Dialog>

      {loading && <Typography sx={{ mt: 2 }}>Loading...</Typography>}
    </Container>
  );
};

export default Departments;