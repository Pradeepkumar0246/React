import React, { useEffect, useMemo, useState } from "react";
import {
  Avatar,
  Box,
  Button,
  Chip,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Pagination,
  Paper,
  Select,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography,
  Checkbox,
} from "@mui/material";
import { Add, Delete, Edit, Visibility, Search } from "@mui/icons-material";
import { useAuth } from "../auth/AuthContext";
import { employeeService } from "../services/employeeService";
import { validateEmail, validateMobileNumber, validateRequired, validateStringLength, validateLettersOnly, validatePassword, validateFileSize, validateFileType } from "../utils/validation";
import type {
  Employee,
  EmployeeFilters,
  Department,
  Role,
} from "../types/employee";

type FormState = {
  firstName: string;
  lastName: string;
  mobileNumber: string;
  personalEmail: string;
  officeEmail: string;
  departmentId: string;
  roleId: string;
  employmentType: string;
  dateOfJoining: string;
  password: string;
  reportingManagerId: string;
};
const rowsPerPage = 10;

const Employees: React.FC = () => {
  const { user } = useAuth();
  const isAdmin = user?.role === "Admin" || user?.role === "HR Manager";
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [roles, setRoles] = useState<Role[]>([]);
  const [loading, setLoading] = useState(false);
  const [filters, setFilters] = useState<EmployeeFilters>({
    search: "",
    departmentId: "",
    roleId: "",
    status: "",
    fromDate: "",
    toDate: "",
  });
  const [page, setPage] = useState(1);
  const [selected, setSelected] = useState<string[]>([]);
  const [openForm, setOpenForm] = useState(false);
  const [viewOpen, setViewOpen] = useState(false);
  const [editEmployee, setEditEmployee] = useState<Employee | null>(null);
  const [viewEmployee, setViewEmployee] = useState<Employee | null>(null);
  const [form, setForm] = useState<FormState>({
    firstName: "",
    lastName: "",
    mobileNumber: "",
    personalEmail: "",
    officeEmail: "",
    departmentId: "",
    roleId: "",
    employmentType: "FullTime",
    dateOfJoining: "",
    password: "",
    reportingManagerId: "",
  });
  const [file, setFile] = useState<File | null>(null);
  const [errors, setErrors] = useState<{[key: string]: string}>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [emps, depts, rls] = await Promise.all([
        employeeService.getAllEmployees(),
        employeeService.getAllDepartments(),
        employeeService.getAllRoles(),
      ]);
      setEmployees(emps ?? []);
      setDepartments(depts ?? []);
      setRoles(rls ?? []);
    } catch (err) {
      console.error("Load error:", err);
    } finally {
      setLoading(false);
    }
  };

  const filtered = useMemo(
    () =>
      employees.filter((e) => {
        const s = (filters.search || "").toLowerCase();
        const matchSearch =
          e.fullName?.toLowerCase().includes(s) ||
          e.officeEmail?.toLowerCase().includes(s) ||
          e.employeeId?.toLowerCase().includes(s);
        return (
          matchSearch &&
          (!filters.departmentId ||
            String(e.departmentId) === String(filters.departmentId)) &&
          (!filters.roleId || String(e.roleId) === String(filters.roleId)) &&
          (!filters.status || e.status === filters.status) &&
          (!filters.fromDate ||
            new Date(e.dateOfJoining) >= new Date(filters.fromDate)) &&
          (!filters.toDate ||
            new Date(e.dateOfJoining) <= new Date(filters.toDate))
        );
      }),
    [employees, filters]
  );

  const paginated = useMemo(
    () => filtered.slice((page - 1) * rowsPerPage, page * rowsPerPage),
    [filtered, page]
  );

  useEffect(() => {
    setSelected((prev) =>
      prev.filter((id) => paginated.some((p) => p.employeeId === id))
    );
  }, [paginated]);

  const handleSelectAll = (checked: boolean) =>
    setSelected(checked ? paginated.map((p) => p.employeeId) : []);
  const handleSelectOne = (id: string, checked: boolean) =>
    setSelected((s) => (checked ? [...s, id] : s.filter((x) => x !== id)));
  const handleView = (emp: Employee) => {
    setViewEmployee(emp);
    setViewOpen(true);
  };
  const handleOpenForm = (emp?: Employee) => {
    setEditEmployee(emp ?? null);
    if (emp) {
      setForm({
        firstName: emp.firstName ?? "",
        lastName: emp.lastName ?? "",
        mobileNumber: emp.mobileNumber ?? "",
        personalEmail: emp.personalEmail ?? "",
        officeEmail: emp.officeEmail ?? "",
        departmentId: String(emp.departmentId ?? ""),
        roleId: String(emp.roleId ?? ""),
        employmentType: emp.employmentType ?? "FullTime",
        dateOfJoining: emp.dateOfJoining ? emp.dateOfJoining.split("T")[0] : "",
        password: "",
        reportingManagerId: emp.reportingManagerId ?? "",
      });
    } else {
      setForm({
        firstName: "",
        lastName: "",
        mobileNumber: "",
        personalEmail: "",
        officeEmail: "",
        departmentId: "",
        roleId: "",
        employmentType: "FullTime",
        dateOfJoining: "",
        password: "",
        reportingManagerId: "",
      });
    }
    setFile(null);
    setOpenForm(true);
  };

  const validateForm = (): boolean => {
    const newErrors: {[key: string]: string} = {};
    
    // Validate required fields
    const firstNameError = validateRequired(form.firstName, 'First Name') || validateStringLength(form.firstName, 2, 50, 'First Name') || validateLettersOnly(form.firstName, 'First Name');
    if (firstNameError) newErrors.firstName = firstNameError;
    
    const lastNameError = validateRequired(form.lastName, 'Last Name') || validateStringLength(form.lastName, 2, 50, 'Last Name') || validateLettersOnly(form.lastName, 'Last Name');
    if (lastNameError) newErrors.lastName = lastNameError;
    
    const mobileError = validateMobileNumber(form.mobileNumber);
    if (mobileError) newErrors.mobileNumber = mobileError;
    
    const personalEmailError = validateEmail(form.personalEmail);
    if (personalEmailError) newErrors.personalEmail = personalEmailError;
    
    const officeEmailError = validateEmail(form.officeEmail);
    if (officeEmailError) newErrors.officeEmail = officeEmailError;
    
    const deptError = validateRequired(form.departmentId, 'Department');
    if (deptError) newErrors.departmentId = deptError;
    
    const roleError = validateRequired(form.roleId, 'Role');
    if (roleError) newErrors.roleId = roleError;
    
    const dateError = validateRequired(form.dateOfJoining, 'Date of Joining');
    if (dateError) newErrors.dateOfJoining = dateError;
    
    if (!editEmployee) {
      const passwordError = validatePassword(form.password);
      if (passwordError) newErrors.password = passwordError;
    }
    
    if (file) {
      const fileSizeError = validateFileSize(file, 5);
      if (fileSizeError) newErrors.file = fileSizeError;
      
      const fileTypeError = validateFileType(file, ['jpg', 'jpeg', 'png', 'gif']);
      if (fileTypeError) newErrors.file = fileTypeError;
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async () => {
    if (!validateForm()) return;
    
    try {
      setIsSubmitting(true);
      const fd = new FormData();
      Object.entries(form).forEach(([k, v]) => fd.append(k, v));
      if (file) fd.append("profilePicture", file);
      if (editEmployee)
        await employeeService.updateEmployee(editEmployee.employeeId, fd);
      else await employeeService.createEmployee(fd);
      await loadData();
      setOpenForm(false);
      setErrors({});
    } catch (err) {
      console.error("Save failed:", err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this employee?")) return;
    try {
      await employeeService.deleteEmployee(id);
      await loadData();
    } catch (err) {
      console.error("Delete error:", err);
    }
  };

  const handleBulkDelete = async () => {
    if (!selected.length || !confirm(`Delete ${selected.length} employees?`))
      return;
    try {
      await Promise.all(
        selected.map((id) => employeeService.deleteEmployee(id))
      );
      setSelected([]);
      await loadData();
    } catch (err) {
      console.error("Bulk delete error:", err);
    }
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>
        Employees
      </Typography>

      <Paper sx={{ p: 2, mb: 3 }}>
        <Box
          sx={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))",
            gap: 2,
            alignItems: "center",
          }}
        >
          <TextField
            placeholder="Search by name, email, or ID"
            value={filters.search}
            onChange={(e) =>
              setFilters((s) => ({ ...s, search: e.target.value }))
            }
            InputProps={{
              startAdornment: (
                <Search sx={{ mr: 1, color: "text.secondary" }} />
              ),
            }}
          />
          <FormControl>
            <InputLabel>Department</InputLabel>
            <Select
              value={filters.departmentId ?? ""}
              label="Department"
              onChange={(e) =>
                setFilters((s) => ({ ...s, departmentId: e.target.value }))
              }
            >
              <MenuItem value="">All</MenuItem>
              {departments.map((d) => (
                <MenuItem key={d.departmentId} value={String(d.departmentId)}>
                  {d.departmentName}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          <FormControl>
            <InputLabel>Role</InputLabel>
            <Select
              value={filters.roleId ?? ""}
              label="Role"
              onChange={(e) =>
                setFilters((s) => ({ ...s, roleId: e.target.value }))
              }
            >
              <MenuItem value="">All</MenuItem>
              {roles.map((r) => (
                <MenuItem key={r.roleId} value={String(r.roleId)}>
                  {r.roleName}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          <FormControl>
            <InputLabel>Status</InputLabel>
            <Select
              value={filters.status ?? ""}
              label="Status"
              onChange={(e) =>
                setFilters((s) => ({ ...s, status: e.target.value }))
              }
            >
              <MenuItem value="">All</MenuItem>
              <MenuItem value="Active">Active</MenuItem>
              <MenuItem value="Inactive">Inactive</MenuItem>
            </Select>
          </FormControl>
          <TextField
            type="date"
            label="From"
            value={filters.fromDate ?? ""}
            onChange={(e) =>
              setFilters((s) => ({ ...s, fromDate: e.target.value }))
            }
            InputLabelProps={{ shrink: true }}
          />
          <TextField
            type="date"
            label="To"
            value={filters.toDate ?? ""}
            onChange={(e) =>
              setFilters((s) => ({ ...s, toDate: e.target.value }))
            }
            InputLabelProps={{ shrink: true }}
          />
        </Box>
      </Paper>

      <Box sx={{ display: "flex", justifyContent: "space-between", mb: 2 }}>
        <Box>
          {isAdmin && (
            <Button
              variant="contained"
              startIcon={<Add />}
              sx={{ mr: 2 }}
              onClick={() => handleOpenForm()}
            >
              Add Employee
            </Button>
          )}
          {isAdmin && selected.length > 0 && (
            <Button
              variant="outlined"
              color="error"
              startIcon={<Delete />}
              onClick={handleBulkDelete}
            >
              Delete Selected ({selected.length})
            </Button>
          )}
        </Box>
        <Typography variant="body2" color="text.secondary">
          {filtered.length} employees found
        </Typography>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              {isAdmin && (
                <TableCell padding="checkbox">
                  <Checkbox
                    checked={
                      selected.length === paginated.length &&
                      paginated.length > 0
                    }
                    indeterminate={
                      selected.length > 0 && selected.length < paginated.length
                    }
                    onChange={(e) => handleSelectAll(e.target.checked)}
                  />
                </TableCell>
              )}
              <TableCell>Employee</TableCell>
              <TableCell>Department</TableCell>
              <TableCell>Role</TableCell>
              <TableCell>Employment Type</TableCell>
              <TableCell>Joining Date</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {paginated.map((emp) => (
              <TableRow key={emp.employeeId}>
                {isAdmin && (
                  <TableCell padding="checkbox">
                    <Checkbox
                      checked={selected.includes(emp.employeeId)}
                      onChange={(e) =>
                        handleSelectOne(emp.employeeId, e.target.checked)
                      }
                    />
                  </TableCell>
                )}
                <TableCell>
                  <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
                    <Avatar
                      src={emp.profilePicture}
                      sx={{ width: 40, height: 40 }}
                    >
                      {emp.firstName?.[0]}
                    </Avatar>
                    <Box>
                      <Typography variant="subtitle2">
                        {emp.fullName}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        {emp.employeeId} • {emp.officeEmail}
                      </Typography>
                    </Box>
                  </Box>
                </TableCell>
                <TableCell>{emp.departmentName}</TableCell>
                <TableCell>{emp.roleName}</TableCell>
                <TableCell>{emp.employmentType}</TableCell>
                <TableCell>
                  {new Date(emp.dateOfJoining).toLocaleDateString()}
                </TableCell>
                <TableCell>
                  <Chip
                    label={emp.status}
                    color={emp.status === "Active" ? "success" : "default"}
                    size="small"
                  />
                </TableCell>
                <TableCell>
                  <Box sx={{ display: "flex", gap: 1 }}>
                    <Tooltip title="View Details">
                      <IconButton size="small" onClick={() => handleView(emp)}>
                        <Visibility />
                      </IconButton>
                    </Tooltip>
                    {isAdmin && (
                      <>
                        <Tooltip title="Edit">
                          <IconButton
                            size="small"
                            onClick={() => handleOpenForm(emp)}
                          >
                            <Edit />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton
                            size="small"
                            color="error"
                            onClick={() => handleDelete(emp.employeeId)}
                          >
                            <Delete />
                          </IconButton>
                        </Tooltip>
                      </>
                    )}
                  </Box>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Box sx={{ display: "flex", justifyContent: "center", mt: 3 }}>
        <Pagination
          count={Math.ceil(filtered.length / rowsPerPage)}
          page={page}
          onChange={(_, p) => setPage(p)}
          color="primary"
        />
      </Box>

      <Dialog
        open={openForm}
        onClose={() => setOpenForm(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          {editEmployee ? "Edit Employee" : "Add Employee"}
        </DialogTitle>
        <DialogContent>
          <Box
            sx={{
              display: "grid",
              gridTemplateColumns: "repeat(2, 1fr)",
              gap: 2,
              mt: 1,
            }}
          >
            <TextField
              label="First Name"
              value={form.firstName}
              onChange={(e) => {
                setForm((s) => ({ ...s, firstName: e.target.value }));
                if (errors.firstName) setErrors(prev => ({...prev, firstName: ''}));
              }}
              onBlur={() => {
                const error = validateRequired(form.firstName, 'First Name') || validateStringLength(form.firstName, 2, 50, 'First Name') || validateLettersOnly(form.firstName, 'First Name');
                if (error) setErrors(prev => ({...prev, firstName: error}));
              }}
              error={!!errors.firstName}
              helperText={errors.firstName}
              required
            />
            <TextField
              label="Last Name"
              value={form.lastName}
              onChange={(e) => {
                setForm((s) => ({ ...s, lastName: e.target.value }));
                if (errors.lastName) setErrors(prev => ({...prev, lastName: ''}));
              }}
              onBlur={() => {
                const error = validateRequired(form.lastName, 'Last Name') || validateStringLength(form.lastName, 2, 50, 'Last Name') || validateLettersOnly(form.lastName, 'Last Name');
                if (error) setErrors(prev => ({...prev, lastName: error}));
              }}
              error={!!errors.lastName}
              helperText={errors.lastName}
              required
            />
            <TextField
              label="Mobile Number"
              value={form.mobileNumber}
              onChange={(e) => {
                setForm((s) => ({ ...s, mobileNumber: e.target.value }));
                if (errors.mobileNumber) setErrors(prev => ({...prev, mobileNumber: ''}));
              }}
              onBlur={() => {
                const error = validateMobileNumber(form.mobileNumber);
                if (error) setErrors(prev => ({...prev, mobileNumber: error}));
              }}
              error={!!errors.mobileNumber}
              helperText={errors.mobileNumber}
              required
            />
            <TextField
              label="Personal Email"
              type="email"
              value={form.personalEmail}
              onChange={(e) => {
                setForm((s) => ({ ...s, personalEmail: e.target.value }));
                if (errors.personalEmail) setErrors(prev => ({...prev, personalEmail: ''}));
              }}
              onBlur={() => {
                const error = validateEmail(form.personalEmail);
                if (error) setErrors(prev => ({...prev, personalEmail: error}));
              }}
              error={!!errors.personalEmail}
              helperText={errors.personalEmail}
              required
            />
            <TextField
              label="Office Email"
              type="email"
              value={form.officeEmail}
              onChange={(e) => {
                setForm((s) => ({ ...s, officeEmail: e.target.value }));
                if (errors.officeEmail) setErrors(prev => ({...prev, officeEmail: ''}));
              }}
              onBlur={() => {
                const error = validateEmail(form.officeEmail);
                if (error) setErrors(prev => ({...prev, officeEmail: error}));
              }}
              error={!!errors.officeEmail}
              helperText={errors.officeEmail}
              required
            />
            <FormControl error={!!errors.departmentId}>
              <InputLabel>Department *</InputLabel>
              <Select
                value={form.departmentId}
                label="Department *"
                onChange={(e) => {
                  setForm((s) => ({ ...s, departmentId: e.target.value }));
                  if (errors.departmentId) setErrors(prev => ({...prev, departmentId: ''}));
                }}
              >
                {departments.map((d) => (
                  <MenuItem key={d.departmentId} value={String(d.departmentId)}>
                    {d.departmentName}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <FormControl error={!!errors.roleId}>
              <InputLabel>Role *</InputLabel>
              <Select
                value={form.roleId}
                label="Role *"
                onChange={(e) => {
                  setForm((s) => ({ ...s, roleId: e.target.value }));
                  if (errors.roleId) setErrors(prev => ({...prev, roleId: ''}));
                }}
              >
                {roles.map((r) => (
                  <MenuItem key={r.roleId} value={String(r.roleId)}>
                    {r.roleName}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <FormControl>
              <InputLabel>Employment Type</InputLabel>
              <Select
                value={form.employmentType}
                label="Employment Type"
                onChange={(e) =>
                  setForm((s) => ({ ...s, employmentType: e.target.value }))
                }
              >
                <MenuItem value="FullTime">Full Time</MenuItem>
                <MenuItem value="PartTime">Part Time</MenuItem>
                <MenuItem value="Contract">Contract</MenuItem>
                <MenuItem value="Intern">Intern</MenuItem>
              </Select>
            </FormControl>
            <TextField
              type="date"
              label="Date of Joining"
              value={form.dateOfJoining}
              onChange={(e) => {
                setForm((s) => ({ ...s, dateOfJoining: e.target.value }));
                if (errors.dateOfJoining) setErrors(prev => ({...prev, dateOfJoining: ''}));
              }}
              error={!!errors.dateOfJoining}
              helperText={errors.dateOfJoining}
              InputLabelProps={{ shrink: true }}
              required
            />
            {!editEmployee && (
              <TextField
                type="password"
                label="Password"
                value={form.password}
                onChange={(e) => {
                  setForm((s) => ({ ...s, password: e.target.value }));
                  if (errors.password) setErrors(prev => ({...prev, password: ''}));
                }}
                onBlur={() => {
                  const error = validatePassword(form.password);
                  if (error) setErrors(prev => ({...prev, password: error}));
                }}
                error={!!errors.password}
                helperText={errors.password}
                required
              />
            )}
            <FormControl>
              <InputLabel>Reporting Manager</InputLabel>
              <Select
                value={form.reportingManagerId}
                label="Reporting Manager"
                onChange={(e) =>
                  setForm((s) => ({ ...s, reportingManagerId: e.target.value }))
                }
              >
                <MenuItem value="">None</MenuItem>
                {employees.map((e) => (
                  <MenuItem key={e.employeeId} value={e.employeeId}>
                    {e.fullName}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <TextField
              type="file"
              label="Profile Picture"
              InputLabelProps={{ shrink: true }}
              inputProps={{ accept: "image/*" }}
              onChange={(e) => {
                setFile((e.target as HTMLInputElement).files?.[0] ?? null);
                if (errors.file) setErrors(prev => ({...prev, file: ''}));
              }}
              error={!!errors.file}
              helperText={errors.file || 'JPG, PNG, GIF only. Max 5MB'}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenForm(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSubmit} disabled={isSubmitting}>
            {isSubmitting ? 'Saving...' : (editEmployee ? "Update" : "Create")}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={viewOpen}
        onClose={() => setViewOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>Employee Details</DialogTitle>
        <DialogContent>
          {viewEmployee && (
            <Box sx={{ mt: 1 }}>
              <Box sx={{ textAlign: "center", mb: 2 }}>
                <Avatar
                  src={viewEmployee.profilePicture}
                  sx={{ width: 80, height: 80, mx: "auto", mb: 1 }}
                >
                  {viewEmployee.firstName?.[0]}
                </Avatar>
                <Typography variant="h6">{viewEmployee.fullName}</Typography>
                <Typography variant="body2" color="text.secondary">
                  {viewEmployee.employeeId}
                </Typography>
              </Box>
              <Box
                sx={{
                  display: "grid",
                  gridTemplateColumns: "repeat(2, 1fr)",
                  gap: 2,
                }}
              >
                {[
                  ["Personal Email", viewEmployee.personalEmail],
                  ["Office Email", viewEmployee.officeEmail],
                  ["Mobile", viewEmployee.mobileNumber],
                  ["Department", viewEmployee.departmentName],
                  ["Role", viewEmployee.roleName],
                  ["Employment Type", viewEmployee.employmentType],
                  [
                    "Joining Date",
                    new Date(viewEmployee.dateOfJoining).toLocaleDateString(),
                  ],
                  ["Status", viewEmployee.status],
                ].map(([label, value]) => (
                  <Typography key={label as string}>
                    <strong>{label}:</strong> {value}
                  </Typography>
                ))}
              </Box>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setViewOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      {loading && <Typography sx={{ mt: 2 }}>Loading...</Typography>}
    </Container>
  );
};

export default Employees;
