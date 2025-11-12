import React from 'react';
import { Box, Typography, Button, Container } from '@mui/material';
import { Lock } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

const UnauthorizedPage: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const isHR = user?.role === 'Admin' || user?.role === 'HR Manager';

  const handleGoHome = () => {
    navigate(isHR ? '/dashboard' : '/employee/dashboard');
  };

  return (
    <Container maxWidth="sm" sx={{ py: 8, textAlign: 'center' }}>
      <Box sx={{ mb: 4 }}>
        <Lock sx={{ fontSize: 80, color: 'error.main', mb: 2 }} />
        <Typography variant="h4" gutterBottom>Access Denied</Typography>
        <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
          You don't have permission to access this page.
        </Typography>
        <Button variant="contained" onClick={handleGoHome}>
          Go to Dashboard
        </Button>
      </Box>
    </Container>
  );
};

export default UnauthorizedPage;