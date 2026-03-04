import 'package:connectivity_plus/connectivity_plus.dart';

class NetworkInfo {
  final Connectivity _connectivity = Connectivity();
  
  Future<bool> get isConnected async {
    final result = await _connectivity.checkConnectivity();
    return result.contains(ConnectivityResult.wifi) || 
           result.contains(ConnectivityResult.ethernet) ||
           result.contains(ConnectivityResult.mobile);
  }
  
  Stream<List<ConnectivityResult>> get onConnectivityChanged {
    return _connectivity.onConnectivityChanged;
  }
}
