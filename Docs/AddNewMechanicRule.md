# Add New Mechanic Rule

File này là rule cần đưa cho Codex mỗi khi muốn thêm một mechanic mới vào game.
Mục tiêu là giữ mechanic mới đi đúng kiến trúc hiện tại, không quay lại kiểu truy cập trực tiếp dictionary hoặc tự scan path riêng lẻ.

## 1. Đọc Các File Nền Trước

Trước khi sửa code, phải đọc các file này:

- `Assets/Scripts/GamePlay/Grid/Core/GridOccupancyContracts.cs`
- `Assets/Scripts/GamePlay/Grid/Core/GridOccupantBehaviour.cs`
- `Assets/Scripts/Manager/Gameplay/GridManager.cs`
- `Assets/Scripts/GamePlay/Grid/Rules/CellOccupancy.cs`
- `Assets/Scripts/GamePlay/Grid/Rules/ObstacleHit.cs`
- `Assets/Scripts/GamePlay/Grid/Rules/MoveResult.cs`
- `Assets/Scripts/GamePlay/Grid/Rules/PathScanner.cs`
- `Assets/Scripts/GamePlay/Snake/SnakeBlock.cs`
- `Assets/Scripts/GamePlay/Snake/SnakeInteractions.cs`

Sau đó đọc thêm 1-3 mechanic gần giống mechanic cần thêm, ví dụ:

- `Assets/Scripts/GamePlay/Grid/Occupants/GridKeycard.cs`
- `Assets/Scripts/GamePlay/Grid/Occupants/GridStopBlock.cs`
- `Assets/Scripts/GamePlay/Grid/Occupants/GridBlackHole.cs`
- `Assets/Scripts/GamePlay/Grid/Occupants/GridElectricWall.cs`
- `Assets/Scripts/GamePlay/Grid/Occupants/GridDeflector.cs`

## 2. Chọn Đúng Loại Mechanic

Trước khi code, phải phân loại mechanic:

- Nếu mechanic chiếm một ô grid: implement `IGridOccupant`, thường nên kế thừa `GridOccupantBehaviour`.
- Nếu mechanic được kích hoạt khi snake đi vào ô đó: implement `IGridTrigger`.
- Nếu mechanic cần biết khi arrow/snake thoát ra khỏi block: implement `IArrowExitListener`.
- Nếu mechanic chặn đường đi: cập nhật rule layer trong `ObstacleHit`, `CellOccupancy`, và `PathScanner`.
- Nếu mechanic đổi hướng, teleport, warp, hoặc cho path tiếp tục sau khi chạm: xử lý trong `PathScanner`, không viết scan riêng trong class visual hoặc MonoBehaviour khác.
- Nếu mechanic chỉ là visual, không ảnh hưởng luật grid/path: không thêm vào rule layer.
- Nếu mechanic chiếm nhiều ô: không ép dùng base một-ô nếu không phù hợp; đăng ký từng cell qua API của `GridManager`.

## 3. Vị Trí File

Đặt file theo vai trò:

- Grid occupant/mechanic: `Assets/Scripts/GamePlay/Grid/Occupants/`
- Luật thuần C# cho board/path: `Assets/Scripts/GamePlay/Grid/Rules/`
- Contract/base grid: `Assets/Scripts/GamePlay/Grid/Core/`
- Logic liên quan snake runtime/movement/input/interaction: `Assets/Scripts/GamePlay/Snake/`
- Manager dùng chung: `Assets/Scripts/Manager/Gameplay/`

Không đặt mechanic mới lung tung ở root `GamePlay` nếu nó thuộc grid hoặc snake.

## 4. Rule Cho GridManager

Không được truy cập dictionary nội bộ của `GridManager` trực tiếp.

Luôn dùng API dạng:

- `Register(...)`
- `Unregister(...)`
- `TryGetObstacle(...)`
- `TryGet...At(...)`
- `TriggerAt(...)`
- `RegisterSnakeCells(...)`
- `UnregisterSnakeCells(...)`
- `ClearLevelState()`

Nếu mechanic mới cần lưu trong `GridManager`:

- Thêm private map/list phù hợp.
- Thêm method register/unregister/query rõ nghĩa.
- Cập nhật `ClearLevelState()`.
- Nếu object có thể bị destroy mà chưa unregister, dùng pattern cleanup giống các method `TryGet...` hiện có.

Không tạo public dictionary mới.

## 5. Rule Cho GridOccupantBehaviour

Nếu mechanic là object một ô grid thông thường, ưu tiên kế thừa `GridOccupantBehaviour`.

Pattern vòng đời nên là:

- Register khi `Start` hoặc `OnEnable`.
- Unregister khi `OnDisable` hoặc `OnDestroy`.
- Nếu cần chờ `GridManager.Instance`, dùng helper sẵn có của base class thay vì tự viết coroutine lặp lại.

Không copy-paste logic register/unregister từ mechanic cũ nếu base class đã xử lý được.

## 6. Rule Cho PathScanner

`PathScanner` là nguồn sự thật cho dự đoán path và khoảng cách va chạm.

Không được viết lại logic scan riêng trong:

- `SnakeBlock`
- `HintManager`
- `ArrowGuideline`
- mechanic visual class

Nếu mechanic ảnh hưởng đường đi, phải mở rộng chung qua:

- `CellOccupancy`
- `ObstacleHit`
- `MoveResult`
- `PathScanner.Scan(...)`
- `PathScanner.BuildGuidelineSegments(...)` nếu mechanic ảnh hưởng guideline

Giữ đúng semantics khoảng cách:

- Obstacle chặn đường thường trả về số bước đi được trước khi chạm: `distance - 1`.
- Trigger/entry có thể đi vào cell, như black hole, có thể trả về `distance`.
- Mechanic đổi hướng/warp thì scanner phải tiếp tục scan theo hướng/vị trí mới.

## 7. Rule Cho Snake

Không nhét toàn bộ mechanic mới vào `SnakeBlock` nếu nó chỉ là trigger hoặc occupant.

Ưu tiên flow:

- `SnakeMover` xử lý movement/coroutine.
- `SnakeInteractions` xử lý trigger/event khi snake dừng, vào ô, thoát ô, key/button/blackhole/stop.
- `SnakeRuntime` giữ state.
- `SnakeRenderer2D` xử lý LineRenderer/arrow/visual.
- `SnakeBlock` chỉ nên làm facade/bridge với serialized fields và các component còn lại.

Nếu mechanic mới chỉ cần phản ứng khi snake vào một ô, hãy implement `IGridTrigger` và để `GridManager.TriggerAt(...)` gọi nó.

Chỉ sửa `SnakeBlock` khi mechanic thật sự thay đổi contract movement hoặc cần xử lý một `ObstacleHitType` mới.

## 8. Rule Cho Event Và Listener

Nếu mechanic cần nghe arrow/snake exit:

- Implement `IArrowExitListener`.
- Đăng ký qua API của `GridManager`.
- Unregister đúng vòng đời.

Không tự tạo thêm event global nếu đã có contract/interface phù hợp.

Nếu cần event mới thật sự dùng chung nhiều nơi, đặt nó ở manager phù hợp và ghi rõ lý do.

## 9. Checklist Khi Thêm Mechanic

Khi thêm mechanic mới, phải kiểm tra:

- Mechanic có register/unregister đúng lifecycle không?
- `ClearLevelState()` có dọn state mới không?
- Rule path có dùng chung `PathScanner` không?
- Hint và guideline có ra kết quả giống movement thật không?
- Không có public dictionary mới trong `GridManager`.
- Không có direct access vào map nội bộ của `GridManager`.
- Không copy-paste coroutine register/unregister nếu `GridOccupantBehaviour` dùng được.
- Không thêm logic visual vào rule layer thuần C#.
- Không thêm logic gameplay cốt lõi vào class chỉ dùng để render.

## 10. Lệnh Kiểm Tra Bắt Buộc

Sau khi sửa code, chạy:

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
```

Nếu có thay đổi liên quan `GridManager`, kiểm tra direct access/event cũ:

```powershell
rg -n "GridManager\.Instance\.[A-Za-z]+Map|manager\.[A-Za-z]+Map|_registeredManager\.[A-Za-z]+Map|OnArrowExitedEvent|OnKeyCollectedEvent|OnElectricButtonPressedEvent" Assets/Scripts -g "*.cs"
```

Nếu mechanic ảnh hưởng path, phải test thủ công trong Unity:

- Snake di chuyển thật.
- Hint chọn move.
- Guideline preview.
- Trường hợp bị block.
- Trường hợp mechanic trigger.
- Trường hợp reset/reload level.

## 11. Format Báo Cáo Sau Khi Làm

Khi hoàn thành mechanic, Codex phải báo lại ngắn gọn:

- Đã thêm mechanic gì.
- File nào được sửa hoặc tạo.
- Mechanic đăng ký vào grid bằng API nào.
- Có thay đổi rule layer không.
- Build/test đã chạy gì.
- Còn rủi ro hoặc test thủ công nào cần làm trong Unity.

