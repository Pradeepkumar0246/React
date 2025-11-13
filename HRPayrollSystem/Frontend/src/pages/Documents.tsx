import React, { useEffect, useMemo, useState } from 'react';
import { Avatar, Box, Button, Chip, Container, Dialog, DialogActions, DialogContent, DialogTitle, FormControl, IconButton, InputLabel, MenuItem, Paper, Select, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Tooltip, Typography } from '@mui/material';
import { Add, Edit, Delete, Download, Description, CloudUpload, Search } from '@mui/icons-material';
import { useAuth } from '../auth/AuthContext';
import { documentService } from '../services/documentService';
import { employeeService } from '../services/employeeService';
import { validateRequired, validateFileSize, validateFileType } from '../utils/validation';
import type { Document, DocumentFilters } from '../types/document';
import type { Employee } from '../types/employee';

type FormState = { employeeId: string; category: string; file: File | null };

const DocumentsPage: React.FC = () => {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin' || user?.role === 'HR Manager';
  const [documents, setDocuments] = useState<Document[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(false);
  const [filters, setFilters] = useState<DocumentFilters>({ search: '', employeeId: '', category: '' });
  const [openForm, setOpenForm] = useState(false);
  const [editDocument, setEditDocument] = useState<Document | null>(null);
  const [form, setForm] = useState<FormState>({ employeeId: '', category: '', file: null });
  const [errors, setErrors] = useState<{[key: string]: string}>({});

  const categories = ['IDProof', 'Experience', 'Skill', 'Education', 'Contract', 'Offer', 'Medical'];

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [documentsData, employeesData] = await Promise.all([
        isAdmin ? documentService.getAll() : documentService.getByEmployee(user?.employeeId || ''),
        employeeService.getAllEmployees()
      ]);
      setDocuments(documentsData ?? []); setEmployees(employeesData ?? []);
    } catch (err) { console.error('Load error:', err); } finally { setLoading(false); }
  };

  const filtered = useMemo(() => {
    const s = (filters.search || '').toLowerCase();
    return documents.filter((doc) => {
      const matchSearch = doc.employeeName?.toLowerCase().includes(s) || doc.employeeId?.toLowerCase().includes(s) || doc.fileName?.toLowerCase().includes(s);
      return matchSearch && (!filters.employeeId || doc.employeeId === filters.employeeId) && (!filters.category || doc.category === filters.category);
    });
  }, [documents, filters]);

  const handleOpenForm = (document?: Document) => {
    setEditDocument(document ?? null);
    setForm(document ? { employeeId: document.employeeId, category: document.category, file: null } : { employeeId: isAdmin ? '' : user?.employeeId || '', category: '', file: null });
    setOpenForm(true);
  };

  const handleSubmit = async () => {
    try {
      if (!form.file && !editDocument) {
        alert('Please select a file'); return;
      }
      
      if (editDocument) {
        await documentService.update(editDocument.documentId, form.employeeId, form.category, form.file || undefined);
      } else {
        await documentService.upload(form.employeeId, form.category, form.file!);
      }
      await loadData(); setOpenForm(false);
    } catch (err: any) {
      console.error('Save failed:', err);
      alert(`Save failed: ${err.response?.data?.message || err.message}`);
    }
  };

  const handleDelete = async (id: number) => {
    if (confirm('Delete this document?')) {
      try { await documentService.delete(id); await loadData(); } catch (err) { console.error('Delete failed:', err); }
    }
  };

  const handleDownload = async (documentId: number, fileName: string) => {
    try {
      const blob = await documentService.download(documentId);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url; a.download = fileName; a.click();
      URL.revokeObjectURL(url);
    } catch (err: any) {
      console.error('Download failed:', err);
      alert(`Download failed: ${err.response?.data?.message || err.message}`);
    }
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const getCategoryColor = (category: string) => {
    switch (category) {
      case 'IDProof': return 'primary';
      case 'Experience': return 'success';
      case 'Education': return 'info';
      case 'Contract': return 'warning';
      case 'Medical': return 'error';
      default: return 'default';
    }
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>Document Management</Typography>

      <Paper sx={{ p: 2, mb: 3 }}>
        <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 2, alignItems: 'center' }}>
          <TextField placeholder="Search by name, ID, or filename" value={filters.search} onChange={(e) => setFilters(s => ({ ...s, search: e.target.value }))} InputProps={{ startAdornment: <Search sx={{ mr: 1, color: 'text.secondary' }} /> }} />
          {isAdmin && <FormControl><InputLabel>Employee</InputLabel><Select value={filters.employeeId} label="Employee" onChange={(e) => setFilters(s => ({ ...s, employeeId: e.target.value }))}><MenuItem value="">All</MenuItem>{employees.map((e) => <MenuItem key={e.employeeId} value={e.employeeId}>{e.fullName}</MenuItem>)}</Select></FormControl>}
          <FormControl><InputLabel>Category</InputLabel><Select value={filters.category} label="Category" onChange={(e) => setFilters(s => ({ ...s, category: e.target.value }))}><MenuItem value="">All</MenuItem>{categories.map((cat) => <MenuItem key={cat} value={cat}>{cat}</MenuItem>)}</Select></FormControl>
        </Box>
      </Paper>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Box>
          <Button variant="contained" startIcon={<Add />} onClick={() => handleOpenForm()}>Upload Document</Button>
        </Box>
        <Typography variant="body2" color="text.secondary">{filtered.length} documents found</Typography>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Employee</TableCell><TableCell>Category</TableCell><TableCell>File Name</TableCell><TableCell>File Size</TableCell><TableCell>Upload Date</TableCell><TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filtered.map((document) => (
              <TableRow key={document.documentId}>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Avatar sx={{ bgcolor: 'primary.main' }}><Description /></Avatar>
                    <Box><Typography variant="subtitle2">{document.employeeName}</Typography><Typography variant="body2" color="text.secondary">{document.employeeId}</Typography></Box>
                  </Box>
                </TableCell>
                <TableCell><Chip label={document.category} color={getCategoryColor(document.category) as any} size="small" /></TableCell>
                <TableCell>{document.fileName}</TableCell>
                <TableCell>{formatFileSize(document.fileSize)}</TableCell>
                <TableCell>{new Date(document.uploadedDate).toLocaleDateString()}</TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    <Tooltip title="Download"><IconButton size="small" onClick={() => handleDownload(document.documentId, document.fileName)}><Download /></IconButton></Tooltip>
                    {(isAdmin || document.employeeId === user?.employeeId) && <Tooltip title="Edit"><IconButton size="small" onClick={() => handleOpenForm(document)}><Edit /></IconButton></Tooltip>}
                    {isAdmin && <Tooltip title="Delete"><IconButton size="small" onClick={() => handleDelete(document.documentId)}><Delete /></IconButton></Tooltip>}
                  </Box>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={openForm} onClose={() => setOpenForm(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editDocument ? 'Edit Document' : 'Upload Document'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <FormControl><InputLabel>Employee</InputLabel><Select value={form.employeeId} label="Employee" onChange={(e) => setForm(s => ({ ...s, employeeId: e.target.value }))} disabled={!isAdmin}>{employees.map((e) => <MenuItem key={e.employeeId} value={e.employeeId}>{e.fullName}</MenuItem>)}</Select></FormControl>
            <FormControl error={!!errors.category}><InputLabel>Category</InputLabel><Select value={form.category} label="Category" onChange={(e) => { setForm(s => ({ ...s, category: e.target.value })); if (errors.category) setErrors(prev => ({...prev, category: ''})); }} onBlur={() => { const error = validateRequired(form.category, 'Category'); if (error) setErrors(prev => ({...prev, category: error})); }}>{categories.map((cat) => <MenuItem key={cat} value={cat}>{cat}</MenuItem>)}</Select></FormControl>
            <Box>
              <Button variant="outlined" component="label" startIcon={<CloudUpload />} fullWidth sx={{ p: 2, borderStyle: 'dashed' }}>
                {form.file ? form.file.name : editDocument ? 'Replace File (Optional)' : 'Select File'}
                <input type="file" hidden onChange={(e) => { const file = e.target.files?.[0] || null; setForm(s => ({ ...s, file })); if (file) { const sizeError = validateFileSize(file, 10); const typeError = validateFileType(file, ['pdf', 'doc', 'docx', 'jpg', 'jpeg', 'png']); if (sizeError || typeError) setErrors(prev => ({...prev, file: sizeError || typeError})); else if (errors.file) setErrors(prev => ({...prev, file: ''})); } }} accept=".pdf,.doc,.docx,.jpg,.jpeg,.png" />
              </Button>
              {errors.file ? <Typography variant="caption" color="error" sx={{ mt: 1, display: 'block' }}>{errors.file}</Typography> : <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>Supported formats: PDF, DOC, DOCX, JPG, PNG (Max 10MB)</Typography>}
            </Box>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenForm(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSubmit}>{editDocument ? 'Update' : 'Upload'}</Button>
        </DialogActions>
      </Dialog>

      {loading && <Typography sx={{ mt: 2 }}>Loading...</Typography>}
    </Container>
  );
};

export default DocumentsPage;