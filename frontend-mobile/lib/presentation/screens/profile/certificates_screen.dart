import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../data/models/crew_member.dart';
import '../../../data/repositories/crew_repository.dart';
import '../../../core/network/api_client.dart';
import '../../../core/network/network_info.dart';
import '../../../core/cache/cache_manager.dart';
import '../../widgets/common/loading_widget.dart';
import '../../widgets/common/error_widget.dart';

class CertificatesScreen extends StatefulWidget {
  const CertificatesScreen({super.key});

  @override
  State<CertificatesScreen> createState() => _CertificatesScreenState();
}

class _CertificatesScreenState extends State<CertificatesScreen> {
  late Future<CrewMember?> _profileFuture;
  late CrewRepository _crewRepository;

  @override
  void initState() {
    super.initState();
    _crewRepository = CrewRepository(
      apiClient: ApiClient(),
      networkInfo: NetworkInfo(),
      cacheManager: CacheManager(),
    );
    _loadProfile();
  }

  void _loadProfile() {
    _profileFuture = _crewRepository.getMyProfile()
        .then((value) => value as CrewMember?)
        .catchError((e) {
      return null;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('My Certificates'),
      ),
      body: FutureBuilder<CrewMember?>(
        future: _profileFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: LoadingWidget(message: 'Loading certificates...'));
          }

          if (snapshot.hasError) {
            return ErrorDisplayWidget(
              message: 'Failed to load certificates',
              onRetry: () {
                setState(() {
                  _loadProfile();
                });
              },
            );
          }

          final profile = snapshot.data;
          if (profile == null) {
            return const ErrorDisplayWidget(message: 'No certificate data found');
          }

          return RefreshIndicator(
            onRefresh: () async {
              setState(() {
                _loadProfile();
              });
            },
            child: ListView(
              padding: const EdgeInsets.all(16),
              children: [
                // Warning Banner
                if (_hasExpiringOrExpiredCertificates(profile))
                  _buildWarningBanner(profile),

                // STCW Certificate
                _buildCertificateCard(
                  context,
                  title: 'STCW Certificate',
                  icon: Icons.card_membership,
                  number: profile.certificateNumber,
                  issueDate: profile.certificateIssue,
                  expiryDate: profile.certificateExpiry,
                  isExpiring: profile.isCertificateExpiring,
                  isExpired: profile.isCertificateExpired,
                ),

                // Medical Certificate
                _buildCertificateCard(
                  context,
                  title: 'Medical Certificate',
                  icon: Icons.medical_services,
                  issueDate: profile.medicalIssue,
                  expiryDate: profile.medicalExpiry,
                  isExpiring: profile.isMedicalExpiring,
                  isExpired: profile.isMedicalExpired,
                ),

                // Passport
                _buildCertificateCard(
                  context,
                  title: 'Passport',
                  icon: Icons.badge,
                  number: profile.passportNumber,
                  expiryDate: profile.passportExpiry,
                  isExpiring: profile.isPassportExpiring,
                  isExpired: profile.isPassportExpired,
                ),

                // Visa
                if (profile.visaNumber != null || profile.visaExpiry != null)
                  _buildCertificateCard(
                    context,
                    title: 'Visa',
                    icon: Icons.flight,
                    number: profile.visaNumber,
                    expiryDate: profile.visaExpiry,
                    isExpiring: false,
                    isExpired: false,
                  ),

                // Seaman Book
                if (profile.seamanBookNumber != null)
                  _buildCertificateCard(
                    context,
                    title: 'Seaman Book',
                    icon: Icons.menu_book,
                    number: profile.seamanBookNumber,
                    isExpiring: false,
                    isExpired: false,
                  ),
              ],
            ),
          );
        },
      ),
    );
  }

  bool _hasExpiringOrExpiredCertificates(CrewMember profile) {
    return profile.isCertificateExpiring ||
        profile.isCertificateExpired ||
        profile.isMedicalExpiring ||
        profile.isMedicalExpired ||
        profile.isPassportExpiring ||
        profile.isPassportExpired;
  }

  Widget _buildWarningBanner(CrewMember profile) {
    final expiredCount = [
      profile.isCertificateExpired,
      profile.isMedicalExpired,
      profile.isPassportExpired,
    ].where((e) => e).length;

    final expiringCount = [
      profile.isCertificateExpiring,
      profile.isMedicalExpiring,
      profile.isPassportExpiring,
    ].where((e) => e).length;

    Color bgColor = Colors.orange.shade50;
    Color borderColor = Colors.orange.shade300;
    Color textColor = Colors.orange.shade900;
    IconData icon = Icons.warning_amber_rounded;
    String message = '';

    if (expiredCount > 0) {
      bgColor = Colors.red.shade50;
      borderColor = Colors.red.shade300;
      textColor = Colors.red.shade900;
      icon = Icons.error_outline;
      message = '$expiredCount certificate(s) have expired!';
    } else if (expiringCount > 0) {
      message = '$expiringCount certificate(s) expiring soon';
    }

    return Container(
      padding: const EdgeInsets.all(16),
      margin: const EdgeInsets.only(bottom: 16),
      decoration: BoxDecoration(
        color: bgColor,
        border: Border.all(color: borderColor),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        children: [
          Icon(icon, color: textColor),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              message,
              style: TextStyle(
                color: textColor,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildCertificateCard(
    BuildContext context, {
    required String title,
    required IconData icon,
    String? number,
    String? issueDate,
    String? expiryDate,
    required bool isExpiring,
    required bool isExpired,
  }) {
    final dateFormat = DateFormat('dd MMM yyyy');

    Color? statusColor;
    String? statusText;
    IconData? statusIcon;

    if (isExpired) {
      statusColor = Colors.red.shade700;
      statusText = 'EXPIRED';
      statusIcon = Icons.error;
    } else if (isExpiring) {
      statusColor = Colors.orange.shade700;
      statusText = 'EXPIRING SOON';
      statusIcon = Icons.warning;
    } else if (expiryDate != null) {
      statusColor = Colors.green.shade700;
      statusText = 'VALID';
      statusIcon = Icons.check_circle;
    }

    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(icon, color: Theme.of(context).colorScheme.primary),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    title,
                    style: Theme.of(context).textTheme.titleMedium?.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                  ),
                ),
                if (statusText != null)
                  Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 8,
                      vertical: 4,
                    ),
                    decoration: BoxDecoration(
                      color: statusColor!.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(4),
                      border: Border.all(color: statusColor),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(statusIcon, size: 14, color: statusColor),
                        const SizedBox(width: 4),
                        Text(
                          statusText,
                          style: TextStyle(
                            color: statusColor,
                            fontSize: 11,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ],
                    ),
                  ),
              ],
            ),
            if (number != null || issueDate != null || expiryDate != null) ...[
              const SizedBox(height: 12),
              const Divider(height: 1),
              const SizedBox(height: 12),
            ],
            if (number != null)
              _buildInfoRow('Number', number),
            if (issueDate != null)
              _buildInfoRow(
                'Issued',
                dateFormat.format(DateTime.parse(issueDate)),
              ),
            if (expiryDate != null) ...[
              _buildInfoRow(
                'Expires',
                dateFormat.format(DateTime.parse(expiryDate)),
              ),
              if (!isExpired) ...[
                const SizedBox(height: 4),
                _buildDaysRemaining(expiryDate),
              ],
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildInfoRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Row(
        children: [
          SizedBox(
            width: 80,
            child: Text(
              label,
              style: TextStyle(
                color: Colors.grey.shade600,
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
          Expanded(
            child: Text(
              value,
              style: const TextStyle(fontWeight: FontWeight.w600),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildDaysRemaining(String expiryDate) {
    final expiry = DateTime.parse(expiryDate);
    final daysLeft = expiry.difference(DateTime.now()).inDays;

    return Row(
      children: [
        const SizedBox(width: 80),
        Expanded(
          child: Text(
            '$daysLeft days remaining',
            style: TextStyle(
              color: daysLeft <= 90
                  ? Colors.orange.shade700
                  : Colors.grey.shade600,
              fontSize: 12,
              fontStyle: FontStyle.italic,
            ),
          ),
        ),
      ],
    );
  }
}
