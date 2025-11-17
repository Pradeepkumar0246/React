import React from 'react';
import { BrowserRouter as Router, useLocation } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import { CssBaseline, Box } from '@mui/material';
import { AuthProvider, useAuth } from './auth/AuthContext';
import { AppRouter } from './routes';
import Navbar from './components/Navbar';
import { hrTheme } from './theme/theme';

const AppRoutes: React.FC = () => {
  const { isAuthenticated } = useAuth();

  return (
    <ThemeProvider theme={hrTheme}>
      <CssBaseline />
      <Box>
        <Navbar />
        <Box sx={{ pt: isAuthenticated ? 8 : 0 }}>
          <AppRouter />
        </Box>
      </Box>
    </ThemeProvider>
  );
};

function App() {
  return (
    <AuthProvider>
      <Router>
        <AppRoutes />
      </Router>
    </AuthProvider>
  );
}

export default App;
