import { createTheme } from '@mui/material/styles';

export const hrTheme = createTheme({
  palette: {
    primary: {
      main: '#2196F3', // Normal Blue
      light: '#64B5F6', // Light Blue
      dark: '#1976D2', // Dark Blue
      contrastText: '#ffffff',
    },
    secondary: {
      main: '#4CAF50', // Normal Green
      light: '#81C784', // Light Green
      dark: '#388E3C', // Dark Green
      contrastText: '#ffffff',
    },
    success: {
      main: '#4CAF50', // Green
      light: '#81C784',
      dark: '#388E3C',
    },
    info: {
      main: '#2196F3', // Blue
      light: '#64B5F6',
      dark: '#1976D2',
    },
    background: {
      default: '#f8fafc',
      paper: '#ffffff',
    },
    text: {
      primary: '#1a202c',
      secondary: '#4a5568',
    },
  },
  typography: {
    fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
    h4: {
      fontWeight: 600,
      color: '#1a202c',
    },
    h5: {
      fontWeight: 600,
    },
    h6: {
      fontWeight: 600,
    },
  },
  components: {
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 12,
          boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
          '&:hover': {
            boxShadow: '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
          },
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          textTransform: 'none',
          fontWeight: 500,
        },
        contained: {
          boxShadow: '0 2px 4px -1px rgba(0, 0, 0, 0.2)',
          '&:hover': {
            boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.3)',
          },
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          borderRadius: 12,
        },
      },
    },
    MuiTableContainer: {
      styleOverrides: {
        root: {
          borderRadius: 8,
        },
      },
    },
  },
});

// Custom color palette for dashboard cards
export const dashboardColors = {
  blue: {
    light: '#E3F2FD', // Very light blue
    main: '#2196F3',   // Normal blue
    dark: '#1976D2',   // Dark blue
    gradient: 'linear-gradient(135deg, #64B5F6 0%, #2196F3 100%)',
  },
  green: {
    light: '#E8F5E8', // Very light green
    main: '#4CAF50',   // Normal green
    dark: '#388E3C',   // Dark green
    gradient: 'linear-gradient(135deg, #81C784 0%, #4CAF50 100%)',
  },
  blueGreen: {
    gradient: 'linear-gradient(135deg, #2196F3 0%, #4CAF50 100%)',
  },
  lightBlue: {
    gradient: 'linear-gradient(135deg, #E3F2FD 0%, #BBDEFB 100%)',
  },
  lightGreen: {
    gradient: 'linear-gradient(135deg, #E8F5E8 0%, #C8E6C9 100%)',
  },
};