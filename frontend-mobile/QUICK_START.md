🚀 Hướng Dẫn Build Và Chạy Dự Án
Bước 1: Khởi động Database (Edge System)
cd e:\NCKH\Martime_product_v1.1\edge-services
docker compose up -d edge-postgres edge-pgadmin




Bước 2: Restore Backend Dependencies
# Edge services
cd e:\NCKH\Martime_product_v1.1\edge-services
dotnet restore
dotnet build



Bước 3: Install Frontend Dependencies
# Frontend Edge
cd e:\NCKH\Martime_product_v1.1\frontend-edge
npm install




Bước 4: Install Flutter Dependencies (Optional)
cd e:\NCKH\Martime_product_v1.1\frontend-mobile
flutter pub get



Bước 5: Chạy Edge System (Khuyến nghị khởi động trước)
Terminal 1 - Backend API:
cd e:\NCKH\Martime_product_v1.1\edge-services
dotnet run --urls "http://0.0.0.0:5001"

Terminal 2 - Frontend Dashboard:
cd e:\NCKH\Martime_product_v1.1\frontend-edge
npm run dev


Bước 7: Generate Code (BẮT BUỘC - chỉ chạy 1 lần)
cd e:\NCKH\Martime_product_v1.1\frontend-mobile
flutter pub run build_runner build --delete-conflicting-outputs

Option A - Windows Desktop (Nhanh nhất cho test):
flutter run -d windows


