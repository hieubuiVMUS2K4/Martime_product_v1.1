import 'package:flutter/material.dart';

/// Maritime Professional Color Palette
/// Centralized theme colors for consistent UI across the app
/// 
/// Usage: 
///   MaritimeTheme.primary
///   MaritimeTheme.completed
class MaritimeTheme {
  MaritimeTheme._();

  // ============================================
  // PRIMARY COLORS - Navy Blue (trust, reliability)
  // ============================================
  
  /// Primary brand color - Navy Blue
  static const Color primary = Color(0xFF1A3A52);
  
  /// Lighter variant for highlights
  static const Color primaryLight = Color(0xFF2C5F7F);
  
  /// Darker variant for emphasis
  static const Color primaryDark = Color(0xFF0D1F2D);

  // ============================================
  // ACCENT COLORS - Sage Green (professional)
  // ============================================
  
  /// Accent color - Sage Green
  static const Color accent = Color(0xFF6B8E7F);
  
  /// Light accent variant
  static const Color accentLight = Color(0xFF8FA99D);

  // ============================================
  // STATUS COLORS
  // ============================================
  
  /// Completed/Success - Deep Green
  static const Color completed = Color(0xFF4A7C59);
  static const Color success = completed;
  
  /// In Progress/Warning - Amber
  static const Color inProgress = Color(0xFFD97706);
  static const Color warning = inProgress;
  
  /// Overdue/Error - Deep Orange/Red
  static const Color overdue = Color(0xFFC2410C);
  static const Color error = overdue;
  
  /// Mandatory/Critical - Deep Red
  static const Color mandatory = Color(0xFFB91C1C);
  static const Color critical = mandatory;
  
  /// Due Soon - Orange
  static const Color dueSoon = Color(0xFFF59E0B);
  
  /// Pending - Blue
  static const Color pending = Color(0xFF3B82F6);

  // ============================================
  // NEUTRAL COLORS
  // ============================================
  
  /// Very light background
  static const Color surfaceLight = Color(0xFFF8FAFC);
  
  /// Light gray background
  static const Color surface = Color(0xFFF1F5F9);
  
  /// Border color
  static const Color border = Color(0xFFCBD5E1);
  
  /// Primary text - Almost black
  static const Color textPrimary = Color(0xFF0F172A);
  
  /// Secondary text - Medium gray
  static const Color textSecondary = Color(0xFF475569);
  
  /// Tertiary text - Light gray
  static const Color textTertiary = Color(0xFF94A3B8);

  // ============================================
  // PRIORITY COLORS
  // ============================================
  
  static const Color priorityCritical = Color(0xFFDC2626);
  static const Color priorityHigh = Color(0xFFF97316);
  static const Color priorityMedium = Color(0xFF3B82F6);
  static const Color priorityLow = Color(0xFF6B7280);

  // ============================================
  // HELPER METHODS
  // ============================================

  /// Get color for task status
  static Color getStatusColor(String status) {
    switch (status.toUpperCase()) {
      case 'COMPLETED':
      case 'APPROVED':
        return completed;
      case 'IN_PROGRESS':
      case 'STARTED':
        return inProgress;
      case 'OVERDUE':
        return overdue;
      case 'PENDING_APPROVAL':
        return dueSoon;
      case 'RECTIFY':
      case 'REJECTED':
        return error;
      case 'DUE':
      case 'SCHEDULED':
      default:
        return pending;
    }
  }

  /// Get color for priority level
  static Color getPriorityColor(String priority) {
    switch (priority.toUpperCase()) {
      case 'CRITICAL':
        return priorityCritical;
      case 'HIGH':
        return priorityHigh;
      case 'MEDIUM':
      case 'NORMAL':
        return priorityMedium;
      case 'LOW':
      default:
        return priorityLow;
    }
  }

  /// Get background color with opacity for status
  static Color getStatusBackground(String status, {double opacity = 0.1}) {
    return getStatusColor(status).withOpacity(opacity);
  }

  // ============================================
  // THEME DATA
  // ============================================
  
  /// Light theme configuration
  static ThemeData get lightTheme {
    return ThemeData(
      useMaterial3: true,
      colorScheme: ColorScheme.fromSeed(
        seedColor: primary,
        brightness: Brightness.light,
        primary: primary,
        secondary: accent,
        error: error,
        surface: surface,
      ),
      appBarTheme: const AppBarTheme(
        centerTitle: true,
        elevation: 0,
        backgroundColor: primary,
        foregroundColor: Colors.white,
      ),
      cardTheme: const CardThemeData(
        elevation: 2,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.all(Radius.circular(12)),
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(8),
        ),
        filled: true,
        fillColor: surfaceLight,
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: primary,
          foregroundColor: Colors.white,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(8),
          ),
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
        ),
      ),
    );
  }
}
