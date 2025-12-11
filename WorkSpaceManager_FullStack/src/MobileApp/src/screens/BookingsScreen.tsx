import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  RefreshControl,
  ActivityIndicator,
  Alert,
} from 'react-native';
import { useNavigation, useFocusEffect } from '@react-navigation/native';
import bookingService, { Booking } from '../services/bookingService';
import { handleApiError } from '../services/apiClient';

const BookingsScreen = () => {
  const navigation = useNavigation<any>();
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  useFocusEffect(
    React.useCallback(() => {
      loadBookings();
    }, [])
  );

  const loadBookings = async () => {
    try {
      const response = await bookingService.getMyBookings();
      if (response.success && response.data) {
        setBookings(response.data);
      }
    } catch (error) {
      Alert.alert('Error', handleApiError(error));
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  const onRefresh = () => {
    setRefreshing(true);
    loadBookings();
  };

  const handleCheckIn = async (id: string) => {
    try {
      const response = await bookingService.checkIn(id);
      if (response.success) {
        Alert.alert('Success', 'Checked in successfully!');
        loadBookings();
      } else {
        Alert.alert('Error', response.message || 'Failed to check in');
      }
    } catch (error) {
      Alert.alert('Error', handleApiError(error));
    }
  };

  const handleCheckOut = async (id: string) => {
    try {
      const response = await bookingService.checkOut(id);
      if (response.success) {
        Alert.alert('Success', 'Checked out successfully!');
        loadBookings();
      } else {
        Alert.alert('Error', response.message || 'Failed to check out');
      }
    } catch (error) {
      Alert.alert('Error', handleApiError(error));
    }
  };

  const handleCancel = (id: string, resourceName: string) => {
    Alert.alert(
      'Cancel Booking',
      `Are you sure you want to cancel the booking for "${resourceName}"?`,
      [
        { text: 'No', style: 'cancel' },
        {
          text: 'Yes, Cancel',
          style: 'destructive',
          onPress: async () => {
            try {
              const response = await bookingService.cancelBooking(id);
              if (response.success) {
                Alert.alert('Success', 'Booking cancelled successfully');
                loadBookings();
              } else {
                Alert.alert('Error', response.message || 'Failed to cancel booking');
              }
            } catch (error) {
              Alert.alert('Error', handleApiError(error));
            }
          },
        },
      ]
    );
  };

  const formatDateTime = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getStatusColor = (status: string) => {
    const colors: Record<string, string> = {
      Pending: '#f59e0b',
      Confirmed: '#10b981',
      CheckedIn: '#3b82f6',
      CheckedOut: '#6b7280',
      Cancelled: '#ef4444',
      NoShow: '#ef4444',
    };
    return colors[status] || '#6b7280';
  };

  const canCheckIn = (booking: Booking) => {
    if (booking.status !== 'Confirmed') return false;
    const now = new Date();
    const start = new Date(booking.startTime);
    const diffMinutes = (start.getTime() - now.getTime()) / (1000 * 60);
    return diffMinutes <= 30 && diffMinutes >= -60;
  };

  const canCheckOut = (booking: Booking) => {
    return booking.status === 'CheckedIn';
  };

  const canCancel = (booking: Booking) => {
    return ['Pending', 'Confirmed'].includes(booking.status);
  };

  if (loading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color="#667eea" />
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <ScrollView
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
        contentContainerStyle={styles.scrollContent}
      >
        {bookings.length === 0 ? (
          <View style={styles.emptyState}>
            <Text style={styles.emptyIcon}>📅</Text>
            <Text style={styles.emptyTitle}>No Bookings Yet</Text>
            <Text style={styles.emptyText}>
              You haven't made any bookings. Start by booking your first workspace!
            </Text>
            <TouchableOpacity
              style={styles.emptyButton}
              onPress={() => navigation.navigate('CreateBooking')}
            >
              <Text style={styles.emptyButtonText}>Book a Space</Text>
            </TouchableOpacity>
          </View>
        ) : (
          <View style={styles.bookingsList}>
            {bookings.map((booking) => (
              <View key={booking.id} style={styles.bookingCard}>
                <View style={styles.cardHeader}>
                  <View style={styles.cardHeaderLeft}>
                    <Text style={styles.resourceType}>{booking.resourceType}</Text>
                    <Text style={styles.resourceName} numberOfLines={1}>
                      {booking.resourceName || `Resource ${booking.resourceId.substring(0, 8)}`}
                    </Text>
                  </View>
                  <View style={[styles.statusBadge, { backgroundColor: getStatusColor(booking.status) }]}>
                    <Text style={styles.statusText}>{booking.status}</Text>
                  </View>
                </View>

                <View style={styles.cardBody}>
                  <View style={styles.infoRow}>
                    <Text style={styles.infoIcon}>📅</Text>
                    <View style={styles.infoText}>
                      <Text style={styles.infoLabel}>Start</Text>
                      <Text style={styles.infoValue}>{formatDateTime(booking.startTime)}</Text>
                    </View>
                  </View>

                  <View style={styles.infoRow}>
                    <Text style={styles.infoIcon}>⏰</Text>
                    <View style={styles.infoText}>
                      <Text style={styles.infoLabel}>End</Text>
                      <Text style={styles.infoValue}>{formatDateTime(booking.endTime)}</Text>
                    </View>
                  </View>

                  {booking.purpose && (
                    <View style={styles.infoRow}>
                      <Text style={styles.infoIcon}>📝</Text>
                      <View style={styles.infoText}>
                        <Text style={styles.infoLabel}>Purpose</Text>
                        <Text style={styles.infoValue}>{booking.purpose}</Text>
                      </View>
                    </View>
                  )}

                  {booking.checkInTime && (
                    <View style={styles.infoRow}>
                      <Text style={styles.infoIcon}>✅</Text>
                      <View style={styles.infoText}>
                        <Text style={styles.infoLabel}>Checked In</Text>
                        <Text style={styles.infoValue}>{formatDateTime(booking.checkInTime)}</Text>
                      </View>
                    </View>
                  )}

                  {booking.checkOutTime && (
                    <View style={styles.infoRow}>
                      <Text style={styles.infoIcon}>👋</Text>
                      <View style={styles.infoText}>
                        <Text style={styles.infoLabel}>Checked Out</Text>
                        <Text style={styles.infoValue}>{formatDateTime(booking.checkOutTime)}</Text>
                      </View>
                    </View>
                  )}
                </View>

                <View style={styles.cardActions}>
                  {canCheckIn(booking) && (
                    <TouchableOpacity
                      style={[styles.actionButton, styles.checkInButton]}
                      onPress={() => handleCheckIn(booking.id)}
                    >
                      <Text style={styles.actionButtonText}>Check In</Text>
                    </TouchableOpacity>
                  )}

                  {canCheckOut(booking) && (
                    <TouchableOpacity
                      style={[styles.actionButton, styles.checkOutButton]}
                      onPress={() => handleCheckOut(booking.id)}
                    >
                      <Text style={styles.actionButtonText}>Check Out</Text>
                    </TouchableOpacity>
                  )}

                  {canCancel(booking) && (
                    <TouchableOpacity
                      style={[styles.actionButton, styles.cancelButton]}
                      onPress={() => handleCancel(booking.id, booking.resourceName || 'this booking')}
                    >
                      <Text style={[styles.actionButtonText, styles.cancelButtonText]}>Cancel</Text>
                    </TouchableOpacity>
                  )}
                </View>
              </View>
            ))}
          </View>
        )}
      </ScrollView>

      <TouchableOpacity
        style={styles.fab}
        onPress={() => navigation.navigate('CreateBooking')}
      >
        <Text style={styles.fabIcon}>+</Text>
      </TouchableOpacity>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f7fafc',
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  scrollContent: {
    padding: 16,
  },
  bookingsList: {
    gap: 16,
  },
  bookingCard: {
    backgroundColor: '#fff',
    borderRadius: 12,
    padding: 16,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 2,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 16,
    paddingBottom: 12,
    borderBottomWidth: 1,
    borderBottomColor: '#e2e8f0',
  },
  cardHeaderLeft: {
    flex: 1,
  },
  resourceType: {
    fontSize: 12,
    color: '#718096',
    marginBottom: 4,
  },
  resourceName: {
    fontSize: 18,
    fontWeight: '600',
    color: '#1a202c',
  },
  statusBadge: {
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 12,
  },
  statusText: {
    fontSize: 11,
    fontWeight: '600',
    color: '#fff',
  },
  cardBody: {
    gap: 12,
    marginBottom: 16,
  },
  infoRow: {
    flexDirection: 'row',
    alignItems: 'flex-start',
  },
  infoIcon: {
    fontSize: 20,
    marginRight: 12,
  },
  infoText: {
    flex: 1,
  },
  infoLabel: {
    fontSize: 12,
    color: '#718096',
    marginBottom: 2,
  },
  infoValue: {
    fontSize: 14,
    color: '#1a202c',
  },
  cardActions: {
    flexDirection: 'row',
    gap: 8,
  },
  actionButton: {
    flex: 1,
    paddingVertical: 12,
    borderRadius: 8,
    alignItems: 'center',
  },
  checkInButton: {
    backgroundColor: '#10b981',
  },
  checkOutButton: {
    backgroundColor: '#3b82f6',
  },
  cancelButton: {
    backgroundColor: '#fff',
    borderWidth: 2,
    borderColor: '#ef4444',
  },
  actionButtonText: {
    fontSize: 14,
    fontWeight: '600',
    color: '#fff',
  },
  cancelButtonText: {
    color: '#ef4444',
  },
  emptyState: {
    backgroundColor: '#fff',
    borderRadius: 12,
    padding: 48,
    alignItems: 'center',
    marginTop: 40,
  },
  emptyIcon: {
    fontSize: 64,
    marginBottom: 16,
  },
  emptyTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#1a202c',
    marginBottom: 8,
  },
  emptyText: {
    fontSize: 14,
    color: '#718096',
    textAlign: 'center',
    marginBottom: 24,
  },
  emptyButton: {
    backgroundColor: '#667eea',
    paddingHorizontal: 32,
    paddingVertical: 14,
    borderRadius: 8,
  },
  emptyButtonText: {
    fontSize: 16,
    fontWeight: '600',
    color: '#fff',
  },
  fab: {
    position: 'absolute',
    right: 20,
    bottom: 20,
    width: 60,
    height: 60,
    borderRadius: 30,
    backgroundColor: '#667eea',
    justifyContent: 'center',
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.3,
    shadowRadius: 8,
    elevation: 8,
  },
  fabIcon: {
    fontSize: 32,
    color: '#fff',
    fontWeight: 'bold',
  },
});

export default BookingsScreen;
