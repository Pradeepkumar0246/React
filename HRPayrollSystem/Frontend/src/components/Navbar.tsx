import React, { useState } from 'react';
import { AppBar, Toolbar, Typography, Button, Box, Avatar } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import pic1 from '../assets/pic1.jpg';

const Navbar: React.FC = () => {
  const { user, logout, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [imageError, setImageError] = useState(false);

  const handleTitleClick = () => {
    const isHR = user?.role === 'Admin' || user?.role === 'HR Manager';
    navigate(isHR ? '/dashboard' : '/employee/dashboard');
  };

  if (!isAuthenticated) return null;

  return (
    <AppBar position="fixed">
      <Toolbar>
        <Typography variant="h6" sx={{ flexGrow: 1, cursor: 'pointer' }} onClick={handleTitleClick}>
          HR Payroll System
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Typography variant="body2">
            {user?.name}
          </Typography>
          <Avatar 
            src={!imageError && user?.profilePicture ? 
              (user.profilePicture.startsWith('http') ? user.profilePicture : `${import.meta.env.VITE_API_BASE_URL?.replace('/api', '')}${user.profilePicture}`) 
              : pic1
            } 
            alt={user?.name} 
            onError={() => setImageError(true)}
            sx={{ width: 32, height: 32 }} 
          />
          <Button color="inherit" onClick={logout}>
            Logout
          </Button>
        </Box>
      </Toolbar>
    </AppBar>
  );
};

export default Navbar;