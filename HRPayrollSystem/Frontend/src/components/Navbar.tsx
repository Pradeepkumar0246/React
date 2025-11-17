import React, { useState } from 'react';
import { AppBar, Toolbar, Typography, Button, Box, Avatar, Menu, MenuItem, IconButton, Divider, useTheme, useMediaQuery } from '@mui/material';
import { Dashboard, People, Business, Security, AccessTime, BeachAccess, AccountBalance, Description, AttachMoney, Assignment, KeyboardArrowDown, Work, Settings } from '@mui/icons-material';
import MenuIcon from '@mui/icons-material/Menu';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import pic1 from '../assets/pic1.jpg';
import logo from '../assets/Logo.png';

const Navbar: React.FC = () => {
  const { user, logout, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const [imageError, setImageError] = useState(false);
  const [profileAnchor, setProfileAnchor] = useState<null | HTMLElement>(null);
  const [mobileMenuAnchor, setMobileMenuAnchor] = useState<null | HTMLElement>(null);
  const [peopleAnchor, setPeopleAnchor] = useState<null | HTMLElement>(null);
  const [operationsAnchor, setOperationsAnchor] = useState<null | HTMLElement>(null);
  const [financeAnchor, setFinanceAnchor] = useState<null | HTMLElement>(null);
  const [systemAnchor, setSystemAnchor] = useState<null | HTMLElement>(null);

  const isHR = user?.role === 'Admin' || user?.role === 'HR Manager';

  const hrMenuGroups = {
    people: [
      { label: 'Employees', path: '/employees', icon: <People /> },
      { label: 'Departments', path: '/departments', icon: <Business /> },
      { label: 'Roles', path: '/roles', icon: <Security /> },
    ],
    operations: [
      { label: 'Attendance', path: '/attendance', icon: <AccessTime /> },
      { label: 'Leave Requests', path: '/leave-requests', icon: <BeachAccess /> },
    ],
    finance: [
      { label: 'Payroll', path: '/payroll', icon: <AccountBalance /> },
      { label: 'Salary Structure', path: '/salary-structure', icon: <AttachMoney /> },
    ],
    system: [
      { label: 'Documents', path: '/documents', icon: <Description /> },
      { label: 'Audit Logs', path: '/audit-logs', icon: <Assignment /> },
    ]
  };

  const employeeMenuItems = [
    { label: 'Dashboard', path: '/employee/dashboard', icon: <Dashboard /> },
    { label: 'My Payroll', path: '/employee/payroll', icon: <AccountBalance /> },
    { label: 'Leave Requests', path: '/employee/leave-requests', icon: <BeachAccess /> },
    { label: 'Documents', path: '/employee/documents', icon: <Description /> },
  ];

  const handleMenuClick = (path: string) => { navigate(path); setMobileMenuAnchor(null); };
  const handleDropdownClick = (path: string, closeDropdown: () => void) => { navigate(path); closeDropdown(); };
  const isGroupActive = (items: typeof hrMenuGroups.people) => items.some(item => location.pathname === item.path);
  const buttonStyle = (isActive: boolean) => ({ borderRadius: 2, px: 2, backgroundColor: isActive ? 'rgba(255,255,255,0.1)' : 'transparent', '&:hover': { backgroundColor: 'rgba(255,255,255,0.1)' } });

  if (!isAuthenticated) return null;

  return (
    <AppBar position="fixed" sx={{ background: 'linear-gradient(135deg, #2196F3 0%, #4CAF50 100%)' }}>
      <Toolbar>
        <Box sx={{ display: 'flex', alignItems: 'center', cursor: 'pointer' }} onClick={() => navigate(isHR ? '/dashboard' : '/employee/dashboard')}>
          <Avatar src={logo} alt="Company Logo" sx={{ width: 40, height: 40, mr: 1 }} />
          <Typography variant="h6" sx={{ fontWeight: 'bold' }}>Phoenix HR</Typography>
        </Box>

        {!isMobile && (
          <Box sx={{ display: 'flex', ml: 4, gap: 1 }}>
            {isHR ? (
              <>
                <Button color="inherit" startIcon={<Dashboard />} onClick={() => navigate('/dashboard')} sx={buttonStyle(location.pathname === '/dashboard')}>Dashboard</Button>
                <Button color="inherit" startIcon={<People />} endIcon={<KeyboardArrowDown />} onClick={(e) => setPeopleAnchor(e.currentTarget)} sx={buttonStyle(isGroupActive(hrMenuGroups.people))}>People</Button>
                <Button color="inherit" startIcon={<Work />} endIcon={<KeyboardArrowDown />} onClick={(e) => setOperationsAnchor(e.currentTarget)} sx={buttonStyle(isGroupActive(hrMenuGroups.operations))}>Operations</Button>
                <Button color="inherit" startIcon={<AccountBalance />} endIcon={<KeyboardArrowDown />} onClick={(e) => setFinanceAnchor(e.currentTarget)} sx={buttonStyle(isGroupActive(hrMenuGroups.finance))}>Finance</Button>
                <Button color="inherit" startIcon={<Settings />} endIcon={<KeyboardArrowDown />} onClick={(e) => setSystemAnchor(e.currentTarget)} sx={buttonStyle(isGroupActive(hrMenuGroups.system))}>System</Button>
              </>
            ) : (
              employeeMenuItems.map((item) => (
                <Button key={item.path} color="inherit" startIcon={item.icon} onClick={() => navigate(item.path)} sx={buttonStyle(location.pathname === item.path)}>{item.label}</Button>
              ))
            )}
          </Box>
        )}

        <Box sx={{ flexGrow: 1 }} />

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          {isMobile && <IconButton color="inherit" onClick={(e) => setMobileMenuAnchor(e.currentTarget)}><MenuIcon /></IconButton>}
          <Box sx={{ display: 'flex', alignItems: 'center', cursor: 'pointer' }} onClick={(e) => setProfileAnchor(e.currentTarget)}>
            <Avatar src={!imageError && user?.profilePicture ? (user.profilePicture.startsWith('http') ? user.profilePicture : `${import.meta.env.VITE_API_BASE_URL?.replace('/api', '')}${user.profilePicture}`) : pic1} alt={user?.name} onError={() => setImageError(true)} sx={{ width: 32, height: 32, mr: 1 }} />
            {!isMobile && <><Typography variant="body2" sx={{ mr: 0.5 }}>{user?.name}</Typography><KeyboardArrowDown fontSize="small" /></>}
          </Box>
        </Box>

        <Menu anchorEl={profileAnchor} open={Boolean(profileAnchor)} onClose={() => setProfileAnchor(null)} transformOrigin={{ horizontal: 'right', vertical: 'top' }} anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}>
          <MenuItem disabled><Box><Typography variant="subtitle2">{user?.name}</Typography><Typography variant="caption" color="text.secondary">{user?.role}</Typography></Box></MenuItem>
          <Divider />
          <MenuItem onClick={() => { logout(); setProfileAnchor(null); }}>Logout</MenuItem>
        </Menu>

        {Object.entries(hrMenuGroups).map(([groupName, items]) => {
          const anchorMap = { people: peopleAnchor, operations: operationsAnchor, finance: financeAnchor, system: systemAnchor };
          const setAnchorMap = { people: setPeopleAnchor, operations: setOperationsAnchor, finance: setFinanceAnchor, system: setSystemAnchor };
          return (
            <Menu key={groupName} anchorEl={anchorMap[groupName as keyof typeof anchorMap]} open={Boolean(anchorMap[groupName as keyof typeof anchorMap])} onClose={() => setAnchorMap[groupName as keyof typeof setAnchorMap](null)}>
              {items.map((item) => (
                <MenuItem key={item.path} onClick={() => handleDropdownClick(item.path, () => setAnchorMap[groupName as keyof typeof setAnchorMap](null))}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>{item.icon}{item.label}</Box>
                </MenuItem>
              ))}
            </Menu>
          );
        })}

        <Menu anchorEl={mobileMenuAnchor} open={Boolean(mobileMenuAnchor)} onClose={() => setMobileMenuAnchor(null)} transformOrigin={{ horizontal: 'right', vertical: 'top' }} anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}>
          {isHR ? [
            <MenuItem key="dashboard" onClick={() => handleMenuClick('/dashboard')}>Dashboard</MenuItem>,
            <Divider key="divider" />,
            ...Object.entries(hrMenuGroups).flatMap(([groupName, items]) => [
              <MenuItem key={groupName} disabled sx={{ fontWeight: 'bold', textTransform: 'capitalize' }}>{groupName}</MenuItem>,
              ...items.map((item) => (
                <MenuItem key={item.path} onClick={() => handleMenuClick(item.path)} sx={{ pl: 4 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>{item.icon}{item.label}</Box>
                </MenuItem>
              ))
            ])
          ] : (
            employeeMenuItems.map((item) => (
              <MenuItem key={item.path} onClick={() => handleMenuClick(item.path)}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>{item.icon}{item.label}</Box>
              </MenuItem>
            ))
          )}
        </Menu>
      </Toolbar>
    </AppBar>
  );
};

export default Navbar;