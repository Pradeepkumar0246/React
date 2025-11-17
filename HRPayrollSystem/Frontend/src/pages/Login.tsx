import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import {
  Box,
  TextField,
  Button,
  Typography,
  Alert,
  InputAdornment,
  IconButton,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import {
  Visibility,
  VisibilityOff,
  Email,
  Lock,
  BusinessCenter,
  Groups,
} from '@mui/icons-material';
import { useAuth } from '../auth/AuthContext';
import type { LoginRequest } from '../types/auth';

const Login: React.FC = () => {
  const { login, isLoading } = useAuth();
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string>('');

  const {
    register,
    handleSubmit,
    formState: { errors },
    trigger,
  } = useForm<LoginRequest>();

  const onSubmit = async (data: LoginRequest) => {
    try {
      setError('');
      await login(data);
    } catch (err) {
      const error = err as { response?: { data?: { message?: string } } };
      setError(error.response?.data?.message || 'Login failed. Please try again.');
    }
  };

  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  return (
    <Box sx={{ minHeight: '100vh', display: 'flex' }}>
      
      {/* LEFT SIDE */}
      {!isMobile && (
        <Box
          sx={{
            width: '50%',
            background: theme.palette.primary.main,
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            color: 'white',
          }}
        >

          <Box sx={{ textAlign: 'center', zIndex: 2 }}>
            <Box sx={{ display: 'flex', justifyContent: 'center', mb: 3 }}>
              <BusinessCenter sx={{ fontSize: 70, mr: 2 }} />
              <Groups sx={{ fontSize: 70 }} />
            </Box>
            <Typography variant="h3" sx={{ fontWeight: 'bold', mb: 2 }}>
              Phoenix HR Payroll
            </Typography>
            <Typography variant="h6" sx={{ opacity: 0.9 }}>
              Streamline Your Workforce Management
            </Typography>
          </Box>
        </Box>
      )}

      {/* RIGHT SIDE */}
      <Box
        sx={{
          width: isMobile ? '100%' : '50%',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          p: 4,
          backgroundColor: '#f1f5f9',
        }}
      >
        <Box sx={{ width: '100%', maxWidth: 400 }}>
          
          <Typography variant="h4" sx={{ textAlign: 'center', mb: 1 }}>
            Sign In
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', mb: 4 }}>
            Access your HR dashboard
          </Typography>

          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

          <Box component="form" onSubmit={handleSubmit(onSubmit)}>
            
            {/* EMAIL FIELD */}
            <TextField
              fullWidth
              label="Office Email"
              type="email"
              margin="normal"
              {...register('officeEmail', {
                required: 'Email is required',
                pattern: {
                  value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                  message: 'Invalid email format',
                },
              })}
              error={!!errors.officeEmail}
              helperText={errors.officeEmail?.message}
              onBlur={() => trigger('officeEmail')}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <Email />
                  </InputAdornment>
                ),
              }}
            />

            {/* PASSWORD FIELD */}
            <TextField
              fullWidth
              label="Password"
              type={showPassword ? 'text' : 'password'}
              margin="normal"
              {...register('password', {
                required: 'Password is required',
                minLength: { value: 6, message: 'Password must be at least 6 characters' },
              })}
              error={!!errors.password}
              helperText={errors.password?.message}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <Lock />
                  </InputAdornment>
                ),
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton onClick={() => setShowPassword(!showPassword)} edge="end">
                      {showPassword ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />

            {/* BUTTON */}
            <Button
              type="submit"
              fullWidth
              variant="contained"
              size="large"
              disabled={isLoading}
              sx={{
                mt: 3,
                py: 1.2,
                fontSize: '1rem',
              }}
            >
              {isLoading ? 'Signing in...' : 'Sign In'}
            </Button>
          </Box>
        </Box>
      </Box>
    </Box>
  );
};

export default Login;
