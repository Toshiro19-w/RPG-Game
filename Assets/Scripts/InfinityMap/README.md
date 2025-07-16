# Infinity Map System - Hướng dẫn sử dụng

## Tổng quan
Hệ thống Infinity Map được thiết kế để tạo ra một chế độ chơi sinh tồn vô hạn với enemy spawn tự động và hệ thống level progression.

## Các Script đã tạo

### 1. PlayerLevel.cs
**Mục đích**: Quản lý level và experience của người chơi
**Tính năng**:
- Hệ thống EXP và level up
- Tự động điều chỉnh spawn rate của enemy theo level
- Events cho level up và exp change

### 2. InfinityEnemyMovement.cs
**Mục đích**: Di chuyển thông minh cho enemy trong Infinity Map
**Tính năng**:
- Tự động tìm và di chuyển tới Player (không cần detection range)
- Pathfinding với wall following
- Dừng lại khi trong attack range

### 3. InfinityEnemy.cs
**Mục đích**: Quản lý behavior cơ bản của enemy
**Tính năng**:
- Health, damage, attack system
- Scaling theo level của player
- Death animation và exp reward

### 4. InfinityEnemySpawner.cs
**Mục đích**: Spawn enemy trên Floor Tilemap
**Tính năng**:
- Spawn trên Floor Tilemap được chỉ định
- Spawn rate thay đổi theo level player
- Weighted random selection cho enemy types
- Scaling stats theo level

### 5. PlayerHealth.cs (KHÔNG CÒN - SỬ DỤNG TỪ HỆ THỐNG CŨ)
**Lý do xóa**: Sử dụng lại PlayerHealth từ folder Player
**Vị trí**: `Assets/Scripts/Player/PlayerHealth.cs`

### 6. InfinityMapManager.cs
**Mục đích**: Quản lý tổng thể game state
**Tính năng**:
- Game pause/resume
- Game over handling
- Statistics tracking

### 7. InfinityMapUI.cs
**Mục đích**: UI hiển thị thông tin game
**Tính năng**:
- Health bar, exp bar, level display
- Game time, enemy count
- Level up notifications

## Cách Setup

### Bước 1: Setup Scene
1. Tạo scene "Infinity Map"
2. Thêm Floor Tilemap cho việc spawn enemy
3. Đặt Player với tag "Player"

### Bước 2: Setup Player
1. Thêm `PlayerLevel` script vào một GameObject (có thể là GameManager)
2. **Player đã có `PlayerHealth` từ hệ thống cũ** - không cần thêm mới
3. Cấu hình các thông số health, invulnerability time nếu cần

### Bước 3: Setup Enemy Spawning
1. Tạo GameObject trống, thêm `InfinityEnemySpawner` script
2. Gán Floor Tilemap vào trường "Floor Tilemap"
3. Tạo enemy prefabs với `InfinityEnemy` và `InfinityEnemyMovement` scripts
4. Thêm enemy prefabs vào Enemy Spawn Data list

### Bước 4: Setup Enemy Prefabs
1. Tạo prefab cho Skeleton và Slime
2. Thêm các scripts:
   - `EnemyHealth` (từ folder Enemy/Mobs - SỬ DỤNG LẠI)
   - `SkeletonAttack` (cho Skeleton) hoặc `SlimeAttack` (cho Slime)
   - `InfinityEnemyMovement` (movement mới cho Infinity Map)
   - `InfinityEnemy` (wrapper để tương thích)
   - `Animator` (nếu có animation)
   - `Rigidbody2D`
   - `Collider2D`

**QUAN TRỌNG**: 
- **KHÔNG sử dụng** script `EnemyMovement` cũ
- **SỬ DỤNG LẠI** `EnemyHealth`, `SkeletonAttack`, `SlimeAttack` từ hệ thống cũ
- **THÊM** `InfinityEnemyMovement` để enemy tự động chạy tới player

### Bước 5: Setup Manager và UI
1. Tạo GameObject cho `InfinityMapManager`
2. Tạo Canvas và thêm `InfinityMapUI`
3. Setup UI elements (health bar, exp bar, etc.)

## Cấu hình Enemy Spawner

### Enemy Spawn Data:
- **Enemy Type**: Skeleton hoặc Slime
- **Enemy Prefab**: Prefab của enemy (phải có EnemyHealth + Attack script tương ứng)
- **Base Health/Damage/Exp**: Stats cơ bản (chỉ Exp được sử dụng, Health/Damage từ EnemyHealth và Attack scripts)
- **Spawn Weight**: Tỷ lệ spawn (0-1)

**Lưu ý về Spawn Weight**:
- **Skeleton Weight: 0.6** (60% chance)
- **Slime Weight: 0.4** (40% chance)
- Nếu chỉ thấy Skeleton, kiểm tra Slime Weight > 0

### Spawn Settings:
- **Base Spawn Rate**: Thời gian spawn cơ bản (giây)
- **Base Max Enemies**: Số enemy tối đa cơ bản
- **Spawn Radius**: Bán kính spawn (không sử dụng)
- **Min/Max Distance**: Khoảng cách spawn từ player

## Layer Setup
Đảm bảo các layer sau được setup đúng:
- **Wall Layer**: Cho tilemap tường (để enemy tránh)
- **Floor Layer**: Cho tilemap sàn (để spawn enemy)
- **Player Layer**: Cho player
- **Enemy Layer**: Cho enemy

## Workflow của hệ thống

1. **Khởi động**: InfinityMapManager khởi tạo tất cả components
2. **Player Level**: Bắt đầu từ level 1, nhận exp khi giết enemy
3. **Enemy Spawning**: Spawner tạo enemy trên floor tilemap theo thời gian
4. **Enemy Behavior**: Enemy tự động di chuyển tới player và tấn công
5. **Level Progression**: Level tăng → spawn rate nhanh hơn, enemy mạnh hơn
6. **UI Updates**: UI cập nhật realtime health, exp, level, thời gian

## Customization

### Điều chỉnh spawn rate theo level:
```csharp
// Trong PlayerLevel.cs
public float GetEnemySpawnRate(float baseSpawnRate)
{
    float adjustedRate = baseSpawnRate * Mathf.Pow(enemySpawnRateMultiplier, currentLevel - 1);
    return Mathf.Max(adjustedRate, minSpawnRate);
}
```

### Thêm enemy type mới:
1. Thêm vào enum `EnemyType`
2. Tạo prefab mới với `InfinityEnemy` script
3. Thêm vào Enemy Spawn Data trong spawner

### Thay đổi scaling:
```csharp
// Trong InfinityEnemySpawner.cs
[SerializeField] private float healthScaling = 1.2f; // Health tăng 20% mỗi level
[SerializeField] private float damageScaling = 1.1f; // Damage tăng 10% mỗi level
```

## Debug & Testing

### Gizmos:
- Enemy movement: Hiển thị wall detection, pathfinding
- Enemy spawner: Hiển thị spawn radius, tilemap bounds
- Player health: Health bar trên đầu player

### Console Logs:
- Enemy spawn events
- Player damage/death
- Level up events
- Spawning statistics

### Test Commands (có thể thêm):
```csharp
// Thêm vào PlayerLevel.cs để test
[ContextMenu("Add 100 EXP")]
void AddTestExp() { AddExp(100); }

[ContextMenu("Set Level 5")]
void SetTestLevel() { SetLevel(5); }
```

## Performance Tips

1. **Object Pooling**: Có thể implement object pooling cho enemy để tối ưu performance
2. **Tilemap Caching**: Valid spawn positions được cache để tránh recalculate
3. **Enemy Cleanup**: Tự động cleanup enemy đã bị destroy
4. **Update Optimization**: Chỉ update cần thiết trong Update()

## Troubleshooting

### Enemy không spawn:
- Kiểm tra Floor Tilemap đã được gán
- Kiểm tra Player có tag "Player"
- Kiểm tra Enemy Spawn Data đã được setup

### Enemy không di chuyển tới Player:
- Kiểm tra Player tag
- Kiểm tra Wall Layer Mask
- Kiểm tra Rigidbody2D và Collider

## 🐛 Troubleshooting Cụ Thể

### Chỉ thấy Skeleton, không thấy Slime:
1. **Kiểm tra Enemy Spawn Data**:
   ```
   - Slime Enemy Type: Slime
   - Slime Enemy Prefab: Gán prefab Slime
   - Spawn Weight: > 0 (khuyến nghị 0.4-0.5)
   ```

2. **Kiểm tra Slime Prefab**:
   ```
   Components cần thiết:
   ✓ EnemyHealth (từ hệ thống cũ)
   ✓ SlimeAttack (từ hệ thống cũ) 
   ✓ InfinityEnemyMovement (mới)
   ✓ InfinityEnemy (wrapper)
   ✓ Rigidbody2D
   ✓ Collider2D
   ```

3. **Kiểm tra Console Logs**:
   - Tìm log "Spawned Slime at ..."
   - Nếu không có, vấn đề ở spawn weight
   - Nếu có nhưng không thấy, vấn đề ở prefab setup

### Enemy không sử dụng Health/Attack cũ:
- Kiểm tra InfinityEnemy đã được thêm vào prefab
- Kiểm tra EnemyHealth, SkeletonAttack/SlimeAttack có trong prefab không
- Đảm bảo không có script InfinityEnemy cũ (đã được replace)

## 🗑️ Scripts Đã Xóa

### PlayerHealth.cs (từ InfinityMap folder)
**Lý do xóa**: Trùng lặp với PlayerHealth trong folder Player
**Thay thế**: Sử dụng `Assets/Scripts/Player/PlayerHealth.cs`
**Tác động**: 
- InfinityMapManager và InfinityMapUI đã được cập nhật để tìm PlayerHealth từ Player GameObject
- UI health sẽ cập nhật trong Update() thay vì qua events
