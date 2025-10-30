import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Box,
  Typography
} from '@mui/material';

const ReportFormModal = ({ open, onClose, onSubmit, appointments, report = null, isEdit = false }) => {
  const [formData, setFormData] = useState({
    appointmentId: '',
    diagnosis: '',
    symptoms: '',
    treatment: '',
    medications: '',
    notes: '',
    recommendations: ''
  });

  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (report && isEdit) {
      setFormData({
        appointmentId: report.appointmentId || '',
        diagnosis: report.diagnosis || '',
        symptoms: report.symptoms || '',
        treatment: report.treatment || '',
        medications: report.medications || '',
        notes: report.notes || '',
        recommendations: report.recommendations || ''
      });
    } else if (!isEdit) {
      setFormData({
        appointmentId: '',
        diagnosis: '',
        symptoms: '',
        treatment: '',
        medications: '',
        notes: '',
        recommendations: ''
      });
    }
  }, [report, isEdit, open]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    
    // Clear error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  const validateForm = () => {
    const newErrors = {};
    
    if (!formData.appointmentId) {
      newErrors.appointmentId = 'Appointment is required';
    }
    if (!formData.diagnosis.trim()) {
      newErrors.diagnosis = 'Diagnosis is required';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = () => {
    if (validateForm()) {
      onSubmit(formData);
    }
  };

  const handleClose = () => {
    setErrors({});
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
      <DialogTitle>
        <Typography variant="h6" component="div">
          {isEdit ? 'Edit Appointment Report' : 'Create Appointment Report'}
        </Typography>
      </DialogTitle>
      
      <DialogContent>
        <Box sx={{ pt: 2, display: 'flex', flexDirection: 'column', gap: 3 }}>
          {/* Appointment Selection */}
          <FormControl fullWidth error={!!errors.appointmentId}>
            <InputLabel>Select Appointment</InputLabel>
            <Select
              name="appointmentId"
              value={formData.appointmentId}
              onChange={handleChange}
              label="Select Appointment"
            >
              {appointments.map((appointment) => (
                <MenuItem key={appointment.id} value={appointment.id}>
                  {appointment.userName} - {new Date(appointment.appointmentDate).toLocaleDateString()} - {appointment.purpose}
                </MenuItem>
              ))}
            </Select>
            {errors.appointmentId && (
              <Typography variant="caption" color="error">
                {errors.appointmentId}
              </Typography>
            )}
          </FormControl>

          {/* Diagnosis */}
          <TextField
            name="diagnosis"
            label="Diagnosis *"
            value={formData.diagnosis}
            onChange={handleChange}
            multiline
            rows={2}
            fullWidth
            error={!!errors.diagnosis}
            helperText={errors.diagnosis}
          />

          {/* Symptoms */}
          <TextField
            name="symptoms"
            label="Symptoms"
            value={formData.symptoms}
            onChange={handleChange}
            multiline
            rows={2}
            fullWidth
            placeholder="Describe patient symptoms..."
          />

          {/* Treatment */}
          <TextField
            name="treatment"
            label="Treatment"
            value={formData.treatment}
            onChange={handleChange}
            multiline
            rows={2}
            fullWidth
            placeholder="Describe treatment provided..."
          />

          {/* Medications */}
          <TextField
            name="medications"
            label="Medications Prescribed"
            value={formData.medications}
            onChange={handleChange}
            multiline
            rows={2}
            fullWidth
            placeholder="List medications, dosages, and instructions..."
          />

          {/* Notes */}
          <TextField
            name="notes"
            label="Clinical Notes"
            value={formData.notes}
            onChange={handleChange}
            multiline
            rows={2}
            fullWidth
            placeholder="Additional clinical observations..."
          />

          {/* Recommendations */}
          <TextField
            name="recommendations"
            label="Recommendations"
            value={formData.recommendations}
            onChange={handleChange}
            multiline
            rows={2}
            fullWidth
            placeholder="Follow-up instructions, lifestyle recommendations..."
          />
        </Box>
      </DialogContent>

      <DialogActions sx={{ p: 3 }}>
        <Button onClick={handleClose} color="inherit">
          Cancel
        </Button>
        <Button 
          onClick={handleSubmit} 
          variant="contained" 
          color="primary"
          disabled={!formData.appointmentId || !formData.diagnosis.trim()}
        >
          {isEdit ? 'Update Report' : 'Create Report'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default ReportFormModal;