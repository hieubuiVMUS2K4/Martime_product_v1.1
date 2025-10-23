import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../providers/auth_provider.dart';
import '../../providers/task_provider.dart';
import '../../providers/sync_provider.dart';
import '../tasks/task_list_screen.dart';
import '../schedule/schedule_screen.dart';
import '../profile/profile_screen.dart';
import '../settings/settings_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  int _selectedIndex = 0;
  
  @override
  void initState() {
    super.initState();
    // Fetch initial data
    WidgetsBinding.instance.addPostFrameCallback((_) {
      Provider.of<TaskProvider>(context, listen: false).fetchMyTasks();
    });
  }
  
  void _onItemTapped(int index) {
    setState(() {
      _selectedIndex = index;
    });
  }
  
  @override
  Widget build(BuildContext context) {
    final authProvider = Provider.of<AuthProvider>(context);
    final taskProvider = Provider.of<TaskProvider>(context);
    final syncProvider = Provider.of<SyncProvider>(context);
    
    return Scaffold(
      appBar: AppBar(
        title: const Text('Maritime Crew App'),
        actions: [
          // Sync indicator
          if (syncProvider.queueSize > 0)
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 8),
              child: Center(
                child: Badge(
                  label: Text('${syncProvider.queueSize}'),
                  child: Icon(
                    syncProvider.isSyncing
                        ? Icons.sync
                        : Icons.cloud_upload,
                  ),
                ),
              ),
            ),
          
          // Online indicator
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: Icon(
              syncProvider.isOnline
                  ? Icons.wifi
                  : Icons.wifi_off,
              color: syncProvider.isOnline ? Colors.green : Colors.red,
            ),
          ),
        ],
      ),
      drawer: Drawer(
        child: ListView(
          padding: EdgeInsets.zero,
          children: [
            UserAccountsDrawerHeader(
              accountName: Text(authProvider.fullName ?? 'Crew Member'),
              accountEmail: Text(authProvider.crewId ?? 'N/A'),
              currentAccountPicture: CircleAvatar(
                backgroundColor: Colors.white,
                child: Text(
                  authProvider.fullName?.substring(0, 1).toUpperCase() ?? 'C',
                  style: const TextStyle(fontSize: 40),
                ),
              ),
            ),
            ListTile(
              leading: const Icon(Icons.home),
              title: const Text('Home'),
              selected: _selectedIndex == 0,
              onTap: () {
                Navigator.pop(context);
                _onItemTapped(0);
              },
            ),
            ListTile(
              leading: const Icon(Icons.task),
              title: const Text('My Tasks'),
              selected: _selectedIndex == 1,
              onTap: () {
                Navigator.pop(context);
                _onItemTapped(1);
              },
            ),
            ListTile(
              leading: const Icon(Icons.calendar_today),
              title: const Text('Schedule'),
              selected: _selectedIndex == 2,
              onTap: () {
                Navigator.pop(context);
                _onItemTapped(2);
              },
            ),
            ListTile(
              leading: const Icon(Icons.person),
              title: const Text('Profile'),
              selected: _selectedIndex == 3,
              onTap: () {
                Navigator.pop(context);
                _onItemTapped(3);
              },
            ),
            const Divider(),
            ListTile(
              leading: const Icon(Icons.settings),
              title: const Text('Settings'),
              onTap: () {
                Navigator.pop(context);
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => const SettingsScreen(),
                  ),
                );
              },
            ),
            ListTile(
              leading: const Icon(Icons.logout),
              title: const Text('Logout'),
              onTap: () async {
                await authProvider.logout();
                if (context.mounted) {
                  Navigator.pushReplacementNamed(context, '/login');
                }
              },
            ),
          ],
        ),
      ),
      body: IndexedStack(
        index: _selectedIndex,
        children: [
          _buildHomeTab(taskProvider, syncProvider),
          _buildTasksTab(taskProvider),
          _buildScheduleTab(),
          _buildProfileTab(authProvider),
        ],
      ),
      bottomNavigationBar: BottomNavigationBar(
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.home),
            label: 'Home',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.task),
            label: 'Tasks',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.calendar_today),
            label: 'Schedule',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.person),
            label: 'Profile',
          ),
        ],
        currentIndex: _selectedIndex,
        onTap: _onItemTapped,
        type: BottomNavigationBarType.fixed,
      ),
    );
  }
  
  Widget _buildHomeTab(TaskProvider taskProvider, SyncProvider syncProvider) {
    return RefreshIndicator(
      onRefresh: () => taskProvider.fetchMyTasks(),
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Dashboard',
              style: Theme.of(context).textTheme.headlineSmall,
            ),
            const SizedBox(height: 16),
            
            // Stats cards
            Row(
              children: [
                Expanded(
                  child: _buildStatCard(
                    'Pending',
                    taskProvider.pendingTasks.length.toString(),
                    Colors.blue,
                    Icons.pending_actions,
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: _buildStatCard(
                    'Overdue',
                    taskProvider.overdueTasks.length.toString(),
                    Colors.red,
                    Icons.warning,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            Row(
              children: [
                Expanded(
                  child: _buildStatCard(
                    'In Progress',
                    taskProvider.inProgressTasks.length.toString(),
                    Colors.orange,
                    Icons.play_circle,
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: _buildStatCard(
                    'Completed',
                    taskProvider.completedTasks.length.toString(),
                    Colors.green,
                    Icons.check_circle,
                  ),
                ),
              ],
            ),
            
            const SizedBox(height: 24),
            
            // Sync status
            if (syncProvider.queueSize > 0)
              Card(
                color: Colors.orange.shade50,
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Row(
                    children: [
                      Icon(Icons.cloud_upload, color: Colors.orange.shade700),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Text(
                          '${syncProvider.queueSize} items waiting to sync',
                          style: TextStyle(color: Colors.orange.shade900),
                        ),
                      ),
                      if (syncProvider.isOnline)
                        TextButton(
                          onPressed: () => syncProvider.syncQueue(),
                          child: const Text('Sync Now'),
                        ),
                    ],
                  ),
                ),
              ),
          ],
        ),
      ),
    );
  }
  
  Widget _buildStatCard(String label, String value, Color color, IconData icon) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            Icon(icon, size: 32, color: color),
            const SizedBox(height: 8),
            Text(
              value,
              style: TextStyle(
                fontSize: 24,
                fontWeight: FontWeight.bold,
                color: color,
              ),
            ),
            Text(
              label,
              style: TextStyle(
                fontSize: 12,
                color: Colors.grey.shade600,
              ),
            ),
          ],
        ),
      ),
    );
  }
  
  Widget _buildTasksTab(TaskProvider taskProvider) {
    return const TaskListScreen();
  }
  
  Widget _buildScheduleTab() {
    return const ScheduleScreen();
  }
  
  Widget _buildProfileTab(AuthProvider authProvider) {
    return const ProfileScreen();
  }
}
