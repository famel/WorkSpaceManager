import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TextInput,
  TouchableOpacity,
  Alert,
  ActivityIndicator,
  Platform,
} from 'react-native';
import { useNavigation } from '@react-navigation/native';
import bookingService, { CreateBookingRequest } from '../services/bookingService';
import { handleApiError } from '../services/apiClient';

const CreateBookingScreen = () => {
  const navigation = useNavigation();
  const [resourceType, setResourceType] = useState<'Desk' | 'MeetingRoom'>('Desk');
  const [resourceId, setResourceId] = useState('');
  const [startDate, setStartDate] = useState('');
  const [startTime, setStartTime] = useState('');
  const [endDate, setEndDate] = useState('');
  const [endTime, setEndTime] = useState('');
  const [purpose, setPurpose] = useState('');
  const [notes, setNotes] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async () => {
    // Validation
    if (!resourceId.trim()) {
      Alert.alert('Error', 'Please enter a resource ID');
      return;
    }

    if (!startDate || !startTime) {
      Alert.alert('Error', 'Please enter start date and time');
      return;
    }

    if (!endDate || !endTime) {
      Alert.alert('Error', 'Please enter end date and time');
      return;
    }

    try {
      setLoading(true);

      const startDateTime = `${startDate}T${startTime}:00`;
      const endDateTime = `${endDate}T${endTime}:00`;

      const request: CreateBookingRequest = {
        resourceType,
        resourceId: resourceId.trim(),
        startTime: startDateTime,
        endTime: endDateTime,
        purpose: purpose.trim() || undefined,
        notes: notes.trim() || undefined,
      };

      const response = await bookingService.createBooking(request);

      if (response.success) {
        Alert.alert('Success', 'Booking created successfully!', [
          { text: 'OK', onPress: () => navigation.goBack() },
        ]);
      } else {
        Alert.alert('Error', response.message || 'Failed to create booking');
      }
    } catch (error) {
      Alert.alert('Error', handleApiError(error));
    } finally {
      setLoading(false);
    }
  };

  return (
    <ScrollView style={styles.container} contentContainerStyle={styles.scrollContent}>
      {/* Resource Type */}
      <View style={styles.section}>
        <Text style={styles.label}>Resource Type *</Text>
        <View style={styles.segmentControl}>
          <TouchableOpacity
            style={[
              styles.segmentButton,
              resourceType === 'Desk' && styles.segmentButtonActive,
            ]}
            onPress={() => setResourceType('Desk')}
          >
            <Text
              style={[
                styles.segmentText,
                resourceType === 'Desk' && styles.segmentTextActive,
              ]}
            >
              🖥️ Desk
            </Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[
              styles.segmentButton,
              resourceType === 'MeetingRoom' && styles.segmentButtonActive,
            ]}
            onPress={() => setResourceType('MeetingRoom')}
          >
            <Text
              style={[
                styles.segmentText,
                resourceType === 'MeetingRoom' && styles.segmentTextActive,
              ]}
            >
              🏢 Meeting Room
            </Text>
          </TouchableOpacity>
        </View>
      </View>

      {/* Resource ID */}
      <View style={styles.section}>
        <Text style={styles.label}>Resource ID *</Text>
        <TextInput
          style={styles.input}
          value={resourceId}
          onChangeText={setResourceId}
          placeholder="Enter resource ID (e.g., desk-101)"
          placeholderTextColor="#a0aec0"
        />
        <Text style={styles.hint}>
          You can find resource IDs in the space management section
        </Text>
      </View>

      {/* Start Date & Time */}
      <View style={styles.section}>
        <Text style={styles.label}>Start Date & Time *</Text>
        <View style={styles.dateTimeRow}>
          <TextInput
            style={[styles.input, styles.dateInput]}
            value={startDate}
            onChangeText={setStartDate}
            placeholder="YYYY-MM-DD"
            placeholderTextColor="#a0aec0"
          />
          <TextInput
            style={[styles.input, styles.timeInput]}
            value={startTime}
            onChangeText={setStartTime}
            placeholder="HH:MM"
            placeholderTextColor="#a0aec0"
          />
        </View>
      </View>

      {/* End Date & Time */}
      <View style={styles.section}>
        <Text style={styles.label}>End Date & Time *</Text>
        <View style={styles.dateTimeRow}>
          <TextInput
            style={[styles.input, styles.dateInput]}
            value={endDate}
            onChangeText={setEndDate}
            placeholder="YYYY-MM-DD"
            placeholderTextColor="#a0aec0"
          />
          <TextInput
            style={[styles.input, styles.timeInput]}
            value={endTime}
            onChangeText={setEndTime}
            placeholder="HH:MM"
            placeholderTextColor="#a0aec0"
          />
        </View>
      </View>

      {/* Purpose */}
      <View style={styles.section}>
        <Text style={styles.label}>Purpose</Text>
        <TextInput
          style={styles.input}
          value={purpose}
          onChangeText={setPurpose}
          placeholder="e.g., Team meeting, Focus work"
          placeholderTextColor="#a0aec0"
        />
      </View>

      {/* Notes */}
      <View style={styles.section}>
        <Text style={styles.label}>Notes</Text>
        <TextInput
          style={[styles.input, styles.textArea]}
          value={notes}
          onChangeText={setNotes}
          placeholder="Any additional notes..."
          placeholderTextColor="#a0aec0"
          multiline
          numberOfLines={4}
          textAlignVertical="top"
        />
      </View>

      {/* Submit Button */}
      <TouchableOpacity
        style={[styles.submitButton, loading && styles.submitButtonDisabled]}
        onPress={handleSubmit}
        disabled={loading}
      >
        {loading ? (
          <ActivityIndicator color="#fff" />
        ) : (
          <Text style={styles.submitButtonText}>Create Booking</Text>
        )}
      </TouchableOpacity>

      {/* Info Box */}
      <View style={styles.infoBox}>
        <Text style={styles.infoIcon}>💡</Text>
        <Text style={styles.infoText}>
          You can check in 30 minutes before your booking starts and up to 1 hour after.
        </Text>
      </View>
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f7fafc',
  },
  scrollContent: {
    padding: 16,
    paddingBottom: 32,
  },
  section: {
    marginBottom: 24,
  },
  label: {
    fontSize: 16,
    fontWeight: '600',
    color: '#1a202c',
    marginBottom: 8,
  },
  hint: {
    fontSize: 12,
    color: '#718096',
    marginTop: 4,
  },
  input: {
    backgroundColor: '#fff',
    borderWidth: 1,
    borderColor: '#e2e8f0',
    borderRadius: 8,
    padding: 14,
    fontSize: 16,
    color: '#1a202c',
  },
  textArea: {
    minHeight: 100,
    paddingTop: 14,
  },
  dateTimeRow: {
    flexDirection: 'row',
    gap: 12,
  },
  dateInput: {
    flex: 2,
  },
  timeInput: {
    flex: 1,
  },
  segmentControl: {
    flexDirection: 'row',
    backgroundColor: '#e2e8f0',
    borderRadius: 8,
    padding: 4,
  },
  segmentButton: {
    flex: 1,
    paddingVertical: 12,
    borderRadius: 6,
    alignItems: 'center',
  },
  segmentButtonActive: {
    backgroundColor: '#fff',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 2,
  },
  segmentText: {
    fontSize: 14,
    fontWeight: '600',
    color: '#718096',
  },
  segmentTextActive: {
    color: '#667eea',
  },
  submitButton: {
    backgroundColor: '#667eea',
    paddingVertical: 16,
    borderRadius: 8,
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.2,
    shadowRadius: 8,
    elevation: 4,
  },
  submitButtonDisabled: {
    opacity: 0.6,
  },
  submitButtonText: {
    fontSize: 18,
    fontWeight: '600',
    color: '#fff',
  },
  infoBox: {
    flexDirection: 'row',
    backgroundColor: '#eef2ff',
    borderRadius: 8,
    padding: 16,
    marginTop: 24,
    alignItems: 'flex-start',
  },
  infoIcon: {
    fontSize: 20,
    marginRight: 12,
  },
  infoText: {
    flex: 1,
    fontSize: 14,
    color: '#4c51bf',
    lineHeight: 20,
  },
});

export default CreateBookingScreen;
