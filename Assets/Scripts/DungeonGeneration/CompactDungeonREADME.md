# Hệ thống tạo Dungeon kiểu Soul Knight

Hệ thống này cho phép tạo dungeon ngẫu nhiên theo phong cách game Soul Knight, với các phòng kết nối với nhau và hiển thị minimap.

## Cách sử dụng

### 1. Thiết lập Scene

1. Tạo một GameObject trống và đặt tên là "DungeonSystem"
2. Thêm các component sau vào GameObject:
   - `DungeonManager`
   - `SoulKnightDungeonGenerator`
   - `SoulKnightRoomGenerator`
   - `SoulKnightDungeonManager`

3. Tạo một GameObject con của Canvas và đặt tên là "Minimap"
4. Thêm component `SoulKnightMinimap` vào GameObject Minimap
5. Thêm một RawImage vào Minimap để hiển thị bản đồ
6. Thêm một Image vào Minimap để làm marker cho người chơi

### 2. Chuẩn bị Room Templates

1. Tạo các prefab cho các loại phòng khác nhau:
   - Normal Room
   - Boss Room
   - Treasure Room
   - Shop Room
   - Start Room

2. Mỗi prefab phòng nên có:
   - Component `Room`
   - Tilemap cho sàn và tường
   - Collider cho tường
   - Vị trí spawn cho enemies và rewards
   - Vị trí cho cửa ở 4 hướng

3. Tạo prefab cho cửa phòng với component `Door`

### 3. Cấu hình SoulKnightDungeonGenerator

1. Thiết lập `gridSize` (kích thước lưới, thường là 5x5 hoặc 6x6)
2. Thiết lập `roomCount` (số lượng phòng, thường từ 10-15)
3. Điều chỉnh tỷ lệ xuất hiện của các phòng đặc biệt:
   - `treasureRoomChance`
   - `shopRoomChance`
   - `secretRoomChance`

### 4. Cấu hình SoulKnightRoomGenerator

1. Gán các prefab phòng vào các trường tương ứng
2. Thêm các biến thể phòng vào các danh sách tương ứng
3. Thiết lập kích thước phòng và khoảng cách giữa các phòng
4. Thêm các prefab chướng ngại vật và vật phẩm có thể phá hủy

### 5. Cấu hình SoulKnightMinimap

1. Gán RawImage vào trường `minimapImage`
2. Gán marker người chơi vào trường `playerMarker`
3. Điều chỉnh `pixelsPerRoom` để thay đổi kích thước hiển thị của minimap
4. Tùy chỉnh màu sắc cho các loại phòng

### 6. Cấu hình SoulKnightDungeonManager

1. Gán các tham chiếu đến các component khác
2. Thiết lập `dungeonLevel` và `dungeonSeed`
3. Gán prefab người chơi vào trường `playerPrefab`
4. Thiết lập UI tiến trình tạo dungeon

## Cách hoạt động

1. `SoulKnightDungeonManager` khởi tạo quá trình tạo dungeon
2. `SoulKnightDungeonGenerator` tạo cấu trúc dungeon với các phòng được kết nối
3. `SoulKnightRoomGenerator` tạo các phòng thực tế với các mẫu phòng và chướng ngại vật
4. `SoulKnightMinimap` hiển thị bản đồ dungeon và vị trí người chơi

## Tùy chỉnh nâng cao

### Thêm loại phòng mới

1. Thêm loại phòng mới vào enum `Room.RoomType`
2. Tạo prefab cho loại phòng mới
3. Thêm các trường mới vào `SoulKnightRoomGenerator` để lưu trữ prefab và các biến thể
4. Cập nhật phương thức `DetermineRoomType` trong `SoulKnightDungeonGenerator` để xác định khi nào tạo loại phòng mới

### Thêm tính năng khóa cửa

1. Sử dụng phương thức `Lock()` và `Unlock()` trong class `Door`
2. Thêm logic để mở khóa cửa khi người chơi có chìa khóa hoặc hoàn thành điều kiện nào đó

### Thêm tính năng teleport giữa các tầng

1. Tạo một prefab portal đặc biệt
2. Khi người chơi tương tác với portal, gọi phương thức `GoToNextLevel()` trong `SoulKnightDungeonManager`

## Lưu ý

- Đảm bảo tất cả các prefab đều có các component cần thiết
- Kiểm tra kích thước và vị trí của các phòng để đảm bảo chúng khớp với nhau
- Điều chỉnh các tham số để có được trải nghiệm gameplay tốt nhất

## Ví dụ mã để tạo dungeon từ script khác

```csharp
// Tìm SoulKnightDungeonManager
SoulKnightDungeonManager dungeonManager = FindFirstObjectByType<SoulKnightDungeonManager>();

// Tạo dungeon mới với seed cụ thể
if (dungeonManager != null)
{
    dungeonManager.GenerateDungeonWithSeed(12345);
}

// Hoặc chuyển đến level tiếp theo
if (dungeonManager != null)
{
    dungeonManager.GoToNextLevel();
}
```