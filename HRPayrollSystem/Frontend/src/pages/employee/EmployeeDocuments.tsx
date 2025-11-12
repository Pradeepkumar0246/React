import React, { useEffect, useState } from 'react';
import { Box, Button, Chip, Dialog, DialogActions, DialogContent, DialogTitle, FormControl, InputLabel, MenuItem, Paper, Select, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from '@mui/material';
import { Add, CloudUpload, Description, Download } from '@mui/icons-material';
import { useAuth } from '../../auth/AuthContext';
import { documentService } from '../../services/documentService';
import type { Document } from '../../types/document';

type FormState = {
  category: string;
  file: File | null;
};

const EmployeeDocuments: React.FC = () => {
  const { user } = useAuth();
  const [documents, setDocuments] = useState<Document[]>([]);
  const [loading, setLoading] = useState(false);
  const [openForm, setOpenForm] = useState(false);
  const [form, setForm] = useState<FormState>({ category: '', file: null });

  const uploadableCategories = ['Skill', 'Medical'];

  useEffect(() => {
    if (user?.employeeId) loadDocuments();
  }, [user?.employeeId]);

  const loadDocuments = async () => {
    try {
      setLoading(true);
      const data = await documentService.getByEmployee(user?.employeeId || '');
      setDocuments(data || []);
    } catch (err) {
      console.error('Load error:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async () => {
    try {
      if (!form.file) {
        alert('Please select a file');
        return;
      }
      await documentService.upload(user?.employeeId || '', form.category, form.file);
      await loadDocuments();
      setOpenForm(false);
      setForm({ category: '', file: null });
    } catch (err) {
      const error = err as { response?: { data?: { message?: string } }; message?: string };
      console.error('Upload failed:', err);
      alert(`Upload failed: ${error.response?.data?.message || error.message}`);
    }
  };

  const handleDownload = async (documentId: number, fileName: string) => {
    try {
      const blob = await documentService.download(documentId);
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
      case 'Skill': return 'secondary';
      default: return 'default';
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>My Documents</Typography>
      
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Button variant="contained" startIcon={<Add />} onClick={() => setOpenForm(true)}>
          Upload Document
        </Button>
        <Typography variant="body2" color="text.secondary">
          {documents.length} documents found
        </Typography>
      </Box>

      {loading ? (
        <Typography>Loading...</Typography>
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Category</TableCell>
                <TableCell>File Name</TableCell>
                <TableCell>File Size</TableCell>
                <TableCell>Upload Date</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {documents.map((document) => (
                <TableRow key={document.documentId}>
                  <TableCell>
                    <Chip 
                      label={document.category} 
                      color={getCategoryColor(document.category) as 'primary' | 'secondary' | 'success' | 'error' | 'info' | 'warning' | 'default'} 
                      size="small" 
                    />
                  </TableCell>
                  <TableCell>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Description />
                      {document.fileName}
                    </Box>
                  </TableCell>
                  <TableCell>{formatFileSize(document.fileSize)}</TableCell>
                  <TableCell>{new Date(document.uploadedDate).toLocaleDateString()}</TableCell>
                  <TableCell>
                    <Button
                      size="small"
                      startIcon={<Download />}
                      onClick={() => handleDownload(document.documentId, document.fileName)}
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

      <Dialog open={openForm} onClose={() => setOpenForm(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Upload Document</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <FormControl>
              <InputLabel>Category</InputLabel>
              <Select value={form.category} label="Category" onChange={(e) => setForm(s => ({ ...s, category: e.target.value }))}>
                {uploadableCategories.map((cat) => (
                  <MenuItem key={cat} value={cat}>{cat}</MenuItem>
                ))}
              </Select>
            </FormControl>
            <Box>
              <Button
                variant="outlined"
                component="label"
                startIcon={<CloudUpload />}
                fullWidth
                sx={{ p: 2, borderStyle: 'dashed' }}
              >
                {form.file ? form.file.name : 'Select File'}
                <input
                  type="file"
                  hidden
                  onChange={(e) => setForm(s => ({ ...s, file: e.target.files?.[0] || null }))}
                  accept=".pdf,.doc,.docx,.jpg,.jpeg,.png"
                />
              </Button>
              <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                Supported formats: PDF, DOC, DOCX, JPG, PNG (Max 10MB)
              </Typography>
              <Typography variant="caption" color="primary" sx={{ mt: 1, display: 'block' }}>
                Note: You can only upload documents under Skill and Medical categories
              </Typography>
            </Box>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenForm(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSubmit}>Upload</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default EmployeeDocuments;