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
              leading: const Icon(Icons.notifications_active, color: Colors.red),
              title: const Text(
                'Safety Alarms',
                style: TextStyle(fontWeight: FontWeight.bold),
              ),
              onTap: () {
                Navigator.pop(context);
                Navigator.pushNamed(context, '/alarms');
              },
            ),
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
        padding: const EdgeInsets.fromLTRB(16, 12, 16, 16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Welcome section - Optimized for mobile
            _buildWelcomeHeader(context),
            
            const SizedBox(height: 16),
            
            // Urgent alerts banner
            if (taskProvider.overdueTasks.isNotEmpty)
              _buildUrgentAlert(context, taskProvider),
            
            // Task Overview section
            _buildSectionTitle(context, 'Task Overview'),
            const SizedBox(height: 10),
            _buildTaskStatsGrid(taskProvider),
            
            const SizedBox(height: 20),
            
            // Quick Access section
            _buildSectionTitle(context, 'Quick Access'),
            const SizedBox(height: 10),
            _buildQuickAccessGrid(context),
            
            const SizedBox(height: 16),
            
            // Sync status (if applicable)
            if (syncProvider.queueSize > 0)
              _buildSyncStatus(syncProvider),
            
            const SizedBox(height: 8),
          ],
        ),
      ),
    );
  }

  // Welcome header with greeting and time
  Widget _buildWelcomeHeader(BuildContext context) {
    return SizedBox(
      height: 64, // Fixed height to prevent overflow
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Text(
                  _getGreeting(),
                  style: Theme.of(context).textTheme.titleLarge?.copyWith(
                        fontWeight: FontWeight.bold,
                        fontSize: 20,
                      ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
                const SizedBox(height: 2),
                Consumer<AuthProvider>(
                  builder: (context, auth, _) => Text(
                    auth.fullName ?? 'Crew Member',
                    style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                          color: Colors.grey.shade600,
                          fontSize: 14,
                        ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(width: 12),
          // Time badge
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
            decoration: BoxDecoration(
              color: Theme.of(context).colorScheme.primaryContainer,
              borderRadius: BorderRadius.circular(10),
            ),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(
                  _getCurrentDate(),
                  style: TextStyle(
                    fontSize: 10,
                    fontWeight: FontWeight.w600,
                    color: Theme.of(context).colorScheme.onPrimaryContainer,
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  _getCurrentTime(),
                  style: TextStyle(
                    fontSize: 14,
                    fontWeight: FontWeight.bold,
                    color: Theme.of(context).colorScheme.onPrimaryContainer,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  // Urgent alert banner
  Widget _buildUrgentAlert(BuildContext context, TaskProvider taskProvider) {
    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      decoration: BoxDecoration(
        color: Colors.red.shade50,
        borderRadius: BorderRadius.circular(10),
        border: Border.all(color: Colors.red.shade200, width: 1),
      ),
      child: InkWell(
        onTap: () => _onItemTapped(1),
        borderRadius: BorderRadius.circular(10),
        child: Row(
          children: [
            Icon(Icons.warning_amber_rounded, color: Colors.red.shade700, size: 22),
            const SizedBox(width: 10),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Urgent Attention!',
                    style: TextStyle(
                      fontWeight: FontWeight.bold,
                      color: Colors.red.shade900,
                      fontSize: 12,
                    ),
                  ),
                  Text(
                    '${taskProvider.overdueTasks.length} overdue task${taskProvider.overdueTasks.length > 1 ? 's' : ''}',
                    style: TextStyle(color: Colors.red.shade700, fontSize: 11),
                  ),
                ],
              ),
            ),
            Icon(Icons.arrow_forward_ios, color: Colors.red.shade700, size: 12),
          ],
        ),
      ),
    );
  }

  // Section title
  Widget _buildSectionTitle(BuildContext context, String title) {
    return Text(
      title,
      style: Theme.of(context).textTheme.titleMedium?.copyWith(
            fontWeight: FontWeight.bold,
            fontSize: 16,
          ),
    );
  }

  // Task stats grid (2x2)
  Widget _buildTaskStatsGrid(TaskProvider taskProvider) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final cardWidth = (constraints.maxWidth - 8) / 2; // 8 = gap
        const cardHeight = 88.0; // Fixed height for consistency
        
        return Wrap(
          spacing: 8,
          runSpacing: 8,
          children: [
            SizedBox(
              width: cardWidth,
              height: cardHeight,
              child: _buildStatCard(
                'Pending',
                taskProvider.pendingTasks.length.toString(),
                Colors.blue,
                Icons.pending_actions,
              ),
            ),
            SizedBox(
              width: cardWidth,
              height: cardHeight,
              child: _buildStatCard(
                'Overdue',
                taskProvider.overdueTasks.length.toString(),
                Colors.red,
                Icons.warning,
              ),
            ),
            SizedBox(
              width: cardWidth,
              height: cardHeight,
              child: _buildStatCard(
                'In Progress',
                taskProvider.inProgressTasks.length.toString(),
                Colors.orange,
                Icons.play_circle,
              ),
            ),
            SizedBox(
              width: cardWidth,
              height: cardHeight,
              child: _buildStatCard(
                'Completed',
                taskProvider.completedTasks.length.toString(),
                Colors.green,
                Icons.check_circle,
              ),
            ),
          ],
        );
      },
    );
  }

  // Quick access grid (3 columns)
  Widget _buildQuickAccessGrid(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final cardWidth = (constraints.maxWidth - 16) / 3; // 16 = 2 gaps of 8
        const cardHeight = 90.0; // Fixed height
        
        return Wrap(
          spacing: 8,
          runSpacing: 8,
          children: [
            SizedBox(
              width: cardWidth,
              height: cardHeight,
              child: _buildQuickActionCard(
                context,
                'Safety\nAlarms',
                Icons.notifications_active,
                Colors.red,
                '/alarms',
              ),
            ),
            SizedBox(
              width: cardWidth,
              height: cardHeight,
              child: _buildQuickActionCard(
                context,
                'My\nTasks',
                Icons.task_alt,
                Colors.blue,
                null,
                onTap: () => _onItemTapped(1),
              ),
            ),
            SizedBox(
              width: cardWidth,
              height: cardHeight,
              child: _buildQuickActionCard(
                context,
                'Watch\nSchedule',
                Icons.access_time,
                Colors.orange,
                null,
                onTap: () => _onItemTapped(2),
              ),
            ),
          ],
        );
      },
    );
  }

  // Sync status card
  Widget _buildSyncStatus(SyncProvider syncProvider) {
    return Card(
      margin: EdgeInsets.zero,
      color: Colors.orange.shade50,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
        child: Row(
          children: [
            Icon(Icons.cloud_upload, color: Colors.orange.shade700, size: 18),
            const SizedBox(width: 8),
            Expanded(
              child: Text(
                '${syncProvider.queueSize} item${syncProvider.queueSize > 1 ? 's' : ''} pending',
                style: TextStyle(
                  color: Colors.orange.shade900,
                  fontSize: 12,
                  fontWeight: FontWeight.w500,
                ),
              ),
            ),
            if (syncProvider.isOnline)
              TextButton(
                onPressed: () => syncProvider.syncQueue(),
                style: TextButton.styleFrom(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                  minimumSize: Size.zero,
                  tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                ),
                child: const Text('Sync', style: TextStyle(fontSize: 11)),
              ),
          ],
        ),
      ),
    );
  }
  
  Widget _buildStatCard(String label, String value, Color color, IconData icon) {
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: InkWell(
        onTap: () => _onItemTapped(1),
        borderRadius: BorderRadius.circular(12),
        child: Container(
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(12),
            gradient: LinearGradient(
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
              colors: [
                color.withOpacity(0.1),
                color.withOpacity(0.05),
              ],
            ),
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(icon, size: 16, color: color),
              const SizedBox(height: 6),
              FittedBox(
                fit: BoxFit.scaleDown,
                alignment: Alignment.centerLeft,
                child: Text(
                  value,
                  style: TextStyle(
                    fontSize: 20,
                    fontWeight: FontWeight.bold,
                    color: color,
                    height: 1.0,
                  ),
                ),
              ),
              const SizedBox(height: 1),
              Text(
                label,
                style: TextStyle(
                  fontSize: 9,
                  color: Colors.grey.shade700,
                  fontWeight: FontWeight.w500,
                  height: 1.1,
                ),
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildQuickActionCard(
    BuildContext context,
    String title,
    IconData icon,
    Color color,
    String? route, {
    VoidCallback? onTap,
  }) {
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: InkWell(
        onTap: onTap ?? (route != null ? () => Navigator.pushNamed(context, route) : null),
        borderRadius: BorderRadius.circular(12),
        child: Container(
          padding: const EdgeInsets.symmetric(vertical: 6, horizontal: 6),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Container(
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: color.withOpacity(0.1),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: Icon(icon, color: color, size: 22),
              ),
              const SizedBox(height: 4),
              FittedBox(
                fit: BoxFit.scaleDown,
                child: Text(
                  title,
                  textAlign: TextAlign.center,
                  style: TextStyle(
                    fontSize: 9,
                    fontWeight: FontWeight.w600,
                    color: Colors.grey.shade800,
                    height: 1.1,
                  ),
                  maxLines: 2,
                ),
              ),
            ],
          ),
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

  // Helper methods for greetings and time
  String _getGreeting() {
    final hour = DateTime.now().hour;
    if (hour < 12) return 'Good Morning';
    if (hour < 17) return 'Good Afternoon';
    if (hour < 21) return 'Good Evening';
    return 'Good Night';
  }

  String _getCurrentDate() {
    final now = DateTime.now();
    final months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    return '${months[now.month - 1]} ${now.day}';
  }

  String _getCurrentTime() {
    final now = DateTime.now();
    return '${now.hour.toString().padLeft(2, '0')}:${now.minute.toString().padLeft(2, '0')}';
  }
}
