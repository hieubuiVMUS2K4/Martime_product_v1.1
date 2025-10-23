import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../data/models/crew_member.dart';
import '../../../data/repositories/crew_repository.dart';
import '../../../core/network/api_client.dart';
import '../../../core/network/network_info.dart';
import '../../../core/cache/cache_manager.dart';
import '../../widgets/common/loading_widget.dart';
import '../../widgets/common/error_widget.dart';
import 'certificates_screen.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
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
        title: const Text('My Profile'),
        actions: [
          IconButton(
            icon: const Icon(Icons.card_membership),
            onPressed: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) => const CertificatesScreen(),
                ),
              );
            },
            tooltip: 'View Certificates',
          ),
        ],
      ),
      body: FutureBuilder<CrewMember?>(
        future: _profileFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: LoadingWidget(message: 'Loading profile...'));
          }

          if (snapshot.hasError) {
            return ErrorDisplayWidget(
              message: 'Failed to load profile',
              onRetry: () {
                setState(() {
                  _loadProfile();
                });
              },
            );
          }

          final profile = snapshot.data;
          if (profile == null) {
            return const ErrorDisplayWidget(message: 'Profile not found');
          }

          return RefreshIndicator(
            onRefresh: () async {
              setState(() {
                _loadProfile();
              });
            },
            child: SingleChildScrollView(
              physics: const AlwaysScrollableScrollPhysics(),
              child: Column(
                children: [
                  _buildHeader(profile),
                  _buildPersonalInfo(profile),
                  _buildContactInfo(profile),
                  _buildEmergencyContact(profile),
                  _buildDocuments(profile),
                  _buildEmploymentInfo(profile),
                  const SizedBox(height: 20),
                ],
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _buildHeader(CrewMember profile) {
    return Container(
      width: double.infinity,
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.primaryContainer,
      ),
      padding: const EdgeInsets.all(24),
      child: Column(
        children: [
          CircleAvatar(
            radius: 50,
            backgroundColor: Theme.of(context).colorScheme.primary,
            child: Text(
              profile.fullName.substring(0, 1).toUpperCase(),
              style: TextStyle(
                fontSize: 40,
                color: Theme.of(context).colorScheme.onPrimary,
              ),
            ),
          ),
          const SizedBox(height: 16),
          Text(
            profile.fullName,
            style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
          ),
          const SizedBox(height: 4),
          Text(
            profile.position,
            style: Theme.of(context).textTheme.titleMedium?.copyWith(
                  color: Theme.of(context).colorScheme.onPrimaryContainer,
                ),
          ),
          const SizedBox(height: 8),
          Chip(
            label: Text(
              'Crew ID: ${profile.crewId}',
              style: const TextStyle(fontWeight: FontWeight.bold),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildPersonalInfo(CrewMember profile) {
    final dateFormat = DateFormat('dd MMM yyyy');
    return _buildSection(
      title: 'Personal Information',
      icon: Icons.person,
      children: [
        if (profile.nationality != null)
          _buildInfoTile('Nationality', profile.nationality!),
        if (profile.dateOfBirth != null)
          _buildInfoTile(
            'Date of Birth',
            dateFormat.format(DateTime.parse(profile.dateOfBirth!)),
          ),
        if (profile.rank != null) _buildInfoTile('Rank', profile.rank!),
        if (profile.department != null) _buildInfoTile('Department', profile.department!),
      ],
    );
  }

  Widget _buildContactInfo(CrewMember profile) {
    return _buildSection(
      title: 'Contact Information',
      icon: Icons.contact_phone,
      children: [
        if (profile.email != null)
          _buildInfoTile('Email', profile.email!, icon: Icons.email),
        if (profile.phoneNumber != null)
          _buildInfoTile('Phone', profile.phoneNumber!, icon: Icons.phone),
        if (profile.address != null)
          _buildInfoTile('Address', profile.address!, icon: Icons.home),
      ],
    );
  }

  Widget _buildEmergencyContact(CrewMember profile) {
    if (profile.emergencyContact == null) return const SizedBox.shrink();
    
    return _buildSection(
      title: 'Emergency Contact',
      icon: Icons.emergency,
      children: [
        _buildInfoTile('Contact Info', profile.emergencyContact!),
      ],
    );
  }

  Widget _buildDocuments(CrewMember profile) {
    final dateFormat = DateFormat('dd MMM yyyy');
    return _buildSection(
      title: 'Documents',
      icon: Icons.badge,
      children: [
        if (profile.passportNumber != null)
          _buildInfoTile('Passport Number', profile.passportNumber!),
        if (profile.passportExpiry != null)
          _buildInfoTile(
            'Passport Expiry',
            dateFormat.format(DateTime.parse(profile.passportExpiry!)),
            trailing: _buildExpiryWarning(profile.isPassportExpiring, profile.isPassportExpired),
          ),
        if (profile.seamanBookNumber != null)
          _buildInfoTile('Seaman Book Number', profile.seamanBookNumber!),
        if (profile.visaNumber != null)
          _buildInfoTile('Visa Number', profile.visaNumber!),
        if (profile.visaExpiry != null)
          _buildInfoTile(
            'Visa Expiry',
            dateFormat.format(DateTime.parse(profile.visaExpiry!)),
          ),
      ],
    );
  }

  Widget _buildEmploymentInfo(CrewMember profile) {
    final dateFormat = DateFormat('dd MMM yyyy');
    return _buildSection(
      title: 'Employment',
      icon: Icons.work,
      children: [
        _buildInfoTile('Status', profile.isOnboard ? 'ONBOARD' : 'OFFBOARD'),
        if (profile.joinDate != null)
          _buildInfoTile(
            'Join Date',
            dateFormat.format(DateTime.parse(profile.joinDate!)),
          ),
        if (profile.embarkDate != null)
          _buildInfoTile(
            'Embark Date',
            dateFormat.format(DateTime.parse(profile.embarkDate!)),
          ),
        if (profile.disembarkDate != null)
          _buildInfoTile(
            'Disembark Date',
            dateFormat.format(DateTime.parse(profile.disembarkDate!)),
          ),
        if (profile.contractEnd != null)
          _buildInfoTile(
            'Contract End',
            dateFormat.format(DateTime.parse(profile.contractEnd!)),
          ),
      ],
    );
  }

  Widget _buildSection({
    required String title,
    required IconData icon,
    required List<Widget> children,
  }) {
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(icon, size: 20, color: Theme.of(context).colorScheme.primary),
                const SizedBox(width: 8),
                Text(
                  title,
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                ),
              ],
            ),
            const Divider(height: 24),
            ...children,
          ],
        ),
      ),
    );
  }

  Widget _buildInfoTile(String label, String value, {IconData? icon, Widget? trailing}) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (icon != null) ...[
            Icon(icon, size: 16, color: Colors.grey.shade600),
            const SizedBox(width: 8),
          ],
          SizedBox(
            width: 120,
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
          if (trailing != null) trailing,
        ],
      ),
    );
  }

  Widget? _buildExpiryWarning(bool isExpiring, bool isExpired) {
    if (isExpired) {
      return Chip(
        label: const Text('EXPIRED', style: TextStyle(fontSize: 10)),
        backgroundColor: Colors.red.shade100,
        labelStyle: TextStyle(color: Colors.red.shade900),
        padding: EdgeInsets.zero,
      );
    } else if (isExpiring) {
      return Chip(
        label: const Text('EXPIRING', style: TextStyle(fontSize: 10)),
        backgroundColor: Colors.orange.shade100,
        labelStyle: TextStyle(color: Colors.orange.shade900),
        padding: EdgeInsets.zero,
      );
    }
    return null;
  }
}
