import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../providers/auth_provider.dart';
import '../../providers/sync_provider.dart';
import '../../../core/cache/cache_manager.dart';
import '../auth/login_screen.dart';
import 'server_config_screen.dart';

class SettingsScreen extends StatelessWidget {
  const SettingsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final authProvider = Provider.of<AuthProvider>(context);
    final syncProvider = Provider.of<SyncProvider>(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Settings'),
      ),
      body: ListView(
        children: [
          // User Info Section
          _buildSection(
            context,
            title: 'Account',
            children: [
              ListTile(
                leading: CircleAvatar(
                  child: Text(
                    authProvider.fullName?.substring(0, 1).toUpperCase() ?? '?',
                  ),
                ),
                title: Text(authProvider.fullName ?? 'User'),
                subtitle: Text(authProvider.position ?? 'Position'),
                trailing: Text(
                  'ID: ${authProvider.crewId ?? 'N/A'}',
                  style: TextStyle(color: Colors.grey.shade600),
                ),
              ),
            ],
          ),

          // Sync Section
          _buildSection(
            context,
            title: 'Synchronization',
            children: [
              ListTile(
                leading: Icon(
                  syncProvider.isOnline ? Icons.cloud_done : Icons.cloud_off,
                  color: syncProvider.isOnline ? Colors.green : Colors.red,
                ),
                title: const Text('Connection Status'),
                subtitle: Text(syncProvider.isOnline ? 'Online' : 'Offline'),
              ),
              ListTile(
                leading: const Icon(Icons.sync),
                title: const Text('Pending Sync Items'),
                subtitle: Text('${syncProvider.queueSize} items waiting to sync'),
                trailing: syncProvider.isSyncing
                    ? const SizedBox(
                        width: 20,
                        height: 20,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : null,
              ),
              if (syncProvider.queueSize > 0 && syncProvider.isOnline)
                ListTile(
                  leading: const Icon(Icons.cloud_upload),
                  title: const Text('Sync Now'),
                  subtitle: const Text('Upload pending changes to server'),
                  trailing: const Icon(Icons.chevron_right),
                  onTap: syncProvider.isSyncing
                      ? null
                      : () async {
                          await syncProvider.syncQueue();
                          if (context.mounted) {
                            ScaffoldMessenger.of(context).showSnackBar(
                              const SnackBar(content: Text('Sync completed')),
                            );
                          }
                        },
                ),
            ],
          ),

          // Server Section
          _buildSection(
            context,
            title: 'Server',
            children: [
              ListTile(
                leading: const Icon(Icons.dns),
                title: const Text('Server Configuration'),
                subtitle: const Text('Configure API server address'),
                trailing: const Icon(Icons.chevron_right),
                onTap: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (context) => const ServerConfigScreen(),
                    ),
                  );
                },
              ),
            ],
          ),

          // Cache Section
          _buildSection(
            context,
            title: 'Data & Storage',
            children: [
              ListTile(
                leading: const Icon(Icons.storage),
                title: const Text('Clear Cache'),
                subtitle: const Text('Remove all cached data'),
                trailing: const Icon(Icons.delete_outline),
                onTap: () {
                  _showClearCacheDialog(context);
                },
              ),
            ],
          ),

          // About Section
          _buildSection(
            context,
            title: 'About',
            children: [
              const ListTile(
                leading: Icon(Icons.info_outline),
                title: Text('Version'),
                subtitle: Text('1.0.0'),
              ),
              const ListTile(
                leading: Icon(Icons.description),
                title: Text('License'),
                subtitle: Text('Proprietary'),
              ),
            ],
          ),

          // Logout Button
          Padding(
            padding: const EdgeInsets.all(16),
            child: ElevatedButton.icon(
              onPressed: () {
                _showLogoutDialog(context, authProvider);
              },
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.red,
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(vertical: 16),
              ),
              icon: const Icon(Icons.logout),
              label: const Text('Logout'),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSection(
    BuildContext context, {
    required String title,
    required List<Widget> children,
  }) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(16, 24, 16, 8),
          child: Text(
            title.toUpperCase(),
            style: Theme.of(context).textTheme.labelSmall?.copyWith(
                  color: Theme.of(context).colorScheme.primary,
                  fontWeight: FontWeight.bold,
                ),
          ),
        ),
        Card(
          margin: const EdgeInsets.symmetric(horizontal: 16),
          child: Column(
            children: children,
          ),
        ),
      ],
    );
  }

  void _showClearCacheDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Clear Cache'),
        content: const Text(
          'This will remove all cached data. You will need an internet connection to reload data. Continue?',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            onPressed: () async {
              final cacheManager = CacheManager();
              await cacheManager.clearAllCache();
              if (context.mounted) {
                Navigator.pop(context);
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Cache cleared successfully')),
                );
              }
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: Colors.red,
              foregroundColor: Colors.white,
            ),
            child: const Text('Clear'),
          ),
        ],
      ),
    );
  }

  void _showLogoutDialog(BuildContext context, AuthProvider authProvider) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Logout'),
        content: const Text('Are you sure you want to logout?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            onPressed: () async {
              await authProvider.logout();
              if (context.mounted) {
                Navigator.of(context).pushAndRemoveUntil(
                  MaterialPageRoute(builder: (context) => const LoginScreen()),
                  (route) => false,
                );
              }
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: Colors.red,
              foregroundColor: Colors.white,
            ),
            child: const Text('Logout'),
          ),
        ],
      ),
    );
  }
}
