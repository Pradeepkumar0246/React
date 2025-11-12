import React from "react";
import { Box, Typography, Container, Card, CardContent } from "@mui/material";
import {
  People,
  Business,
  Security,
  AccessTime,
  BeachAccess,
  AccountBalance,
  Description,
  Assignment,
  AttachMoney,
} from "@mui/icons-material";
import { useNavigate } from "react-router-dom";
// import { useAuth } from '../auth/AuthContext';

interface ModuleCard {
  title: string;
  description: string;
  icon: React.ReactElement;
  path: string;
  gradient: string;
}

const HRDashboard: React.FC = () => {
  // const { user } = useAuth();
  const navigate = useNavigate();

  const modules: ModuleCard[] = [
    {
      title: "Employees",
      description:
        "Manage employee profiles, personal info, and organizational structure",
      icon: <People sx={{ fontSize: 40 }} />,
      path: "/employees",
      gradient: "linear-gradient(135deg, #4a5568 0%, #2d3748 100%)",
    },
    {
      title: "Departments",
      description: "Organize and manage company departments and hierarchies",
      icon: <Business sx={{ fontSize: 40 }} />,
      path: "/departments",
      gradient: "linear-gradient(135deg, #e53e3e 0%, #c53030 100%)",
    },
    {
      title: "Roles",
      description: "Define job roles, responsibilities, and access permissions",
      icon: <Security sx={{ fontSize: 40 }} />,
      path: "/roles",
      gradient: "linear-gradient(135deg, #3182ce 0%, #2c5282 100%)",
    },
    {
      title: "Attendance",
      description:
        "Track employee check-ins, working hours, and attendance records",
      icon: <AccessTime sx={{ fontSize: 40 }} />,
      path: "/attendance",
      gradient: "linear-gradient(135deg, #38a169 0%, #2f855a 100%)",
    },
    {
      title: "Leave Requests",
      description:
        "Process leave applications, approvals, and balance management",
      icon: <BeachAccess sx={{ fontSize: 40 }} />,
      path: "/leave-requests",
      gradient: "linear-gradient(135deg, #ed8936 0%, #dd6b20 100%)",
    },
    {
      title: "Payroll Management",
      description:
        "Generate payroll, manage salaries, and download payslips",
      icon: <AccountBalance sx={{ fontSize: 40 }} />,
      path: "/payroll",
      gradient: "linear-gradient(135deg, #319795 0%, #2c7a7b 100%)",
    },
    {
      title: "Documents",
      description: "Manage employee documents, contracts, and file uploads",
      icon: <Description sx={{ fontSize: 40 }} />,
      path: "/documents",
      gradient: "linear-gradient(135deg, #805ad5 0%, #6b46c1 100%)",
    },
    {
      title: "Salary Structure",
      description: "Configure salary components, allowances, and deductions",
      icon: <AttachMoney sx={{ fontSize: 40 }} />,
      path: "/salary-structure",
      gradient: "linear-gradient(135deg, #38b2ac 0%, #319795 100%)",
    },
    {
      title: "Audit Logs",
      description: "Monitor system activities, user actions, and security logs",
      icon: <Assignment sx={{ fontSize: 40 }} />,
      path: "/audit-logs",
      gradient: "linear-gradient(135deg, #718096 0%, #4a5568 100%)",
    },
  ];

  const handleCardClick = (path: string) => {
    navigate(path);
  };

  return (
    <Container disableGutters sx={{ py: 4, px: 3 }}>
      <Box
        sx={{
          maxWidth: "1200px",
          margin: "0 auto",
          display: "grid",
          gridTemplateColumns: "repeat(auto-fill, minmax(300px, 1fr))",
          gap: 3,
        }}
      >
        {modules.map((module) => (
          <Card
            key={module.title}
            sx={{
              height: "100%",
              cursor: "pointer",
              transition: "all 0.3s ease",
              background: module.gradient,
              color: "white",
              "&:hover": {
                transform: "translateY(-4px)",
                boxShadow: 6,
              },
            }}
            onClick={() => handleCardClick(module.path)}
          >
            <CardContent sx={{ p: 3, textAlign: "center" }}>
              <Box sx={{ mb: 2 }}>{module.icon}</Box>
              <Typography variant="h6" gutterBottom>
                {module.title}
              </Typography>
              <Typography variant="body2" sx={{ opacity: 0.9 }}>
                {module.description}
              </Typography>
            </CardContent>
          </Card>
        ))}
      </Box>
    </Container>
  );
};

export default HRDashboard;
