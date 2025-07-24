HƯỚNG DẪN THIẾT LẬP WAVE COUNTDOWN UI

1. Tạo UI Canvas:
   - Trong Unity, chuột phải vào Hierarchy -> UI -> Canvas
   - Đảm bảo Canvas Scaler được thiết lập phù hợp (Scale With Screen Size)

2. Tạo Text cho đếm ngược:
   - Chuột phải vào Canvas -> UI -> Text - TextMeshPro
   - Nếu Unity yêu cầu import TMP Essentials, hãy nhấn "Import TMP Essentials"

3. Thiết lập Text:
   - Đặt tên GameObject là "WaveCountdownText"
   - Đặt vị trí ở giữa màn hình (Anchor Presets -> Middle Center)
   - Thiết lập font size lớn (khoảng 72)
   - Chọn màu sắc nổi bật (ví dụ: màu đỏ hoặc vàng)
   - Đặt Alignment là Center
   - Có thể thêm Outline hoặc Shadow để text dễ đọc hơn

4. Thêm component WaveCountdownUI:
   - Chọn Canvas
   - Add Component -> Scripts -> InfinityMap -> WaveCountdownUI
   - Kéo thả TextMeshPro vừa tạo vào trường "Countdown Text"

5. Điều chỉnh các thông số (tùy chọn):
   - Initial Countdown Duration: Thời gian đếm ngược ban đầu (mặc định: 3 giây)
   - Between Waves Countdown Duration: Thời gian đếm ngược giữa các đợt (mặc định: 10 giây)
   - Initial Message: Thông báo ban đầu (mặc định: "Ải này sẽ bắt đầu trong {0}...")
   - Next Wave Message: Thông báo giữa các đợt (mặc định: "Đợt tiếp theo sẽ bắt đầu trong {0}...")

6. Kiểm tra:
   - Chạy game và kiểm tra xem thông báo đếm ngược có hiển thị không
   - Đảm bảo thông báo biến mất sau khi đếm ngược kết thúc
   - Kiểm tra xem thông báo có xuất hiện lại giữa các đợt không

LƯU Ý:
- Đảm bảo Canvas được thiết lập với "Screen Space - Overlay" để hiển thị trên cùng
- Có thể thêm hiệu ứng animation cho text để tăng tính hấp dẫn
- Có thể thêm âm thanh đếm ngược nếu muốn