import React, { useEffect, useState } from 'react';
import { Avatar, Box, Button, Container, Dialog, DialogActions, DialogContent, DialogTitle, FormControl, IconButton, InputLabel, MenuItem, Paper, Select, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Tooltip, Typography } from '@mui/material';
import { Add, Delete, Edit, Work } from '@mui/icons-material';
import { useAuth } from '../auth/AuthContext';
import { roleService } from '../services/roleService';
import { departmentService } from '../services/departmentService';
import type { Role, Department } from '../types/employee';

type FormState = { roleName: string; description: string; departmentId: string };

const Roles: React.FC = () => {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin' || user?.role === 'HR Manager';
  const [roles, setRoles] = useState<Role[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [loading, setLoading] = useState(false);
  const [openForm, setOpenForm] = useState(false);
  const [editRole, setEditRole] = useState<Role | null>(null);
  const [form, setForm] = useState<FormState>({ roleName: '', description: '', departmentId: '' });

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [rolesData, deptsData] = await Promise.all([roleService.getAll(), departmentService.getAll()]);
      setRoles(rolesData ?? []); setDepartments(deptsData ?? []);
    } catch (err) { console.error('Load error:', err); } finally { setLoading(false); }
  };

  const handleOpenForm = (role?: Role) => {
    setEditRole(role ?? null);
    setForm(role ? { roleName: role.roleName, description: role.description || '', departmentId: String(role.departmentId || '') } : { roleName: '', description: '', departmentId: '' });
    setOpenForm(true);
  };

  const handleSubmit = async () => {
    try {
      const data = { ...form, departmentId: form.departmentId ? Number(form.departmentId) : undefined };
      if (editRole) {
        await roleService.update(editRole.roleId, { roleId: editRole.roleId, ...data });
      } else {
        await roleService.create(data);
      }
      await loadData(); setOpenForm(false);
    } catch (err) { console.error('Save failed:', err); }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Delete this role?')) return;
    try { await roleService.delete(id); await loadData(); } catch (err) { console.error('Delete error:', err); }
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>Roles</Typography>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Box>
          {isAdmin && <Button variant="contained" startIcon={<Add />} onClick={() => handleOpenForm()}>Add Role</Button>}
        </Box>
        <Typography variant="body2" color="text.secondary">{roles.length} roles found</Typography>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Role</TableCell><TableCell>Department</TableCell><TableCell>Description</TableCell><TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {roles.map((role) => (
              <TableRow key={role.roleId}>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Avatar sx={{ bgcolor: 'secondary.main' }}><Work /></Avatar>
                    <Typography variant="subtitle2">{role.roleName}</Typography>
                  </Box>
                </TableCell>
                <TableCell>{role.departmentName || 'No department'}</TableCell>
                <TableCell>{role.description || 'No description'}</TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    {isAdmin && (
                      <>
                        <Tooltip title="Edit"><IconButton size="small" onClick={() => handleOpenForm(role)}><Edit /></IconButton></Tooltip>
                        <Tooltip title="Delete"><IconButton size="small" color="error" onClick={() => handleDelete(role.roleId)}><Delete /></IconButton></Tooltip>
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
        <DialogTitle>{editRole ? 'Edit Role' : 'Add Role'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField label="Role Name" value={form.roleName} onChange={(e) => setForm(s => ({ ...s, roleName: e.target.value }))} />
            <FormControl>
              <InputLabel>Department</InputLabel>
              <Select value={form.departmentId} label="Department" onChange={(e) => setForm(s => ({ ...s, departmentId: e.target.value }))}>
                <MenuItem value="">None</MenuItem>
                {departments.map((d) => <MenuItem key={d.departmentId} value={String(d.departmentId)}>{d.departmentName}</MenuItem>)}
              </Select>
            </FormControl>
            <TextField label="Description" multiline rows={3} value={form.description} onChange={(e) => setForm(s => ({ ...s, description: e.target.value }))} />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenForm(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSubmit}>{editRole ? 'Update' : 'Create'}</Button>
        </DialogActions>
      </Dialog>

      {loading && <Typography sx={{ mt: 2 }}>Loading...</Typography>}
    </Container>
  );
};

export default Roles;