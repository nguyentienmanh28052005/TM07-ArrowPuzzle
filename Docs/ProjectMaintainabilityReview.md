# Project Maintainability Review

## Muc tieu

Tai lieu nay tong hop nhanh tinh trang ky thuat cua project `TM07-ArrowPuzzle` theo huong:

- Phan nao nen giu lai vi dang tao nen tang tot cho maintainability.
- Phan nao nen thay doi vi dang la diem nghen khi them mechanic, sua bug, hoac mo rong feature.
- Thu tu uu tien refactor de team co the di tung buoc, khong can dap di lam lai.

Danh gia nay dua tren cac nhom code dang giu gameplay core:

- `Assets/Scripts/GamePlay/Grid/Core/`
- `Assets/Scripts/GamePlay/Grid/Rules/`
- `Assets/Scripts/GamePlay/Grid/Occupants/`
- `Assets/Scripts/GamePlay/Snake/`
- `Assets/Scripts/Manager/Gameplay/GridManager.cs`
- `Assets/Scripts/Manager/Level/LevelLoader.cs`
- `Assets/Scripts/Manager/Level/LevelController.cs`
- `Assets/Scripts/LevelSystem/Runtime/`
- `Assets/Scripts/UI/Screens/GameCanvas.cs`

## Tom tat nhanh

Project hien tai da co mot huong kien truc dung cho gameplay grid:

- Rule scan duong di da duoc dua ve `PathScanner`.
- Occupant da co contract chung qua `IGridOccupant`, `IGridTrigger`, `IArrowExitListener`.
- Cac mechanic moi dang co xu huong dang ky vao `GridManager` thay vi moi noi tu scan rieng.
- `SnakeBlock` da bat dau dong vai tro facade, con movement va runtime duoc tach ra thanh class rieng.

Day la nen tang rat nen giu lai.

Tuy nhien, project van co 4 diem ngheo lon:

1. `GridManager` dang vua la registry, vua la event hub, vua la query service cho qua nhieu mechanic.
2. Nhieu class van phu thuoc truc tiep vao `GridManager.Instance`, `FindObjectOfType`, `MessageManager.Instance`.
3. Mot so class dang qua to va om nhieu trach nhiem, dac biet la `GameCanvas.cs` va `LevelLoader.cs`.
4. Logic gameplay, scene lookup, visual effect, va orchestration van con bi tron trong mot so luong file lon, nen rat de phat sinh bug khi sua.

## Phan nen giu lai

### 1. Rule layer cua grid

Nen giu va tiep tuc mo rong theo huong hien tai:

- `Assets/Scripts/GamePlay/Grid/Rules/PathScanner.cs`
- `Assets/Scripts/GamePlay/Grid/Rules/MoveResult.cs`
- `Assets/Scripts/GamePlay/Grid/Rules/CellOccupancy.cs`
- `Assets/Scripts/GamePlay/Grid/Rules/BoardState.cs`

Ly do nen giu:

- `PathScanner` dang la noi tap trung de scan obstacle, black hole, portal, deflector.
- `MoveResult` va `ObstacleHit` giup gameplay co mot ket qua scan co nghia thay vi tra ve gia tri ro rac.
- `CellOccupancy` dang dong vai tro adapter giua rule thuần C# va `GridManager`.
- `BoardState` tuy don gian nhung la huong dung de sau nay tach rule khoi scene state.

Tac dong ve lau dai:

- Khi them mechanic moi, team co mot diem vao ro rang cho logic path.
- Hint, movement, guideline co co hoi dung chung cung mot nguon su that.

### 2. Contract cho mechanic grid

Nen giu:

- `Assets/Scripts/GamePlay/Grid/Core/GridOccupancyContracts.cs`
- `Assets/Scripts/GamePlay/Grid/Core/GridOccupantBehaviour.cs`

Ly do nen giu:

- Mechanic moi da co mau ro rang: chiem o, trigger khi vao o, hay lang nghe luc arrow thoat.
- `GridOccupantBehaviour` da giam dang ke viec copy-paste logic register/unregister.

Tac dong ve lau dai:

- Them mechanic moi se de review hon.
- Giam kha nang quen unregister va gay state rac trong `GridManager`.

### 3. Tach `SnakeBlock` thanh runtime, mover, renderer, interactions

Nen giu huong tach nay:

- `Assets/Scripts/GamePlay/Snake/SnakeBlock.cs`
- `Assets/Scripts/GamePlay/Snake/SnakeMover.cs`
- `Assets/Scripts/GamePlay/Snake/SnakeRuntime.cs`
- `Assets/Scripts/GamePlay/Snake/SnakeInteractions.cs`

Ly do nen giu:

- `SnakeBlock` khong con om het logic nhu mot MonoBehaviour khong lo.
- `SnakeMover` quan ly flow movement.
- `SnakeRuntime` giu state va tinh toan track/grid occupancy.
- `SnakeInteractions` dong vai tro cau noi giua runtime va feedback/trigger.

Nhan xet:

- Day la huong refactor dung, can tiep tuc day manh.
- Chua can viet lai cum nay tu dau.

### 4. Data-driven level runtime

Nen giu:

- `Assets/Scripts/LevelSystem/Runtime/LevelRuntimeBuilderV2.cs`
- `Assets/Scripts/LevelSystem/Runtime/LevelRuntimeFactoryV2.cs`

Ly do nen giu:

- Builder va factory da tao duoc diem vao ro rang tu data sang runtime object.
- Day la nen tang tot cho level editor, playtest, procedural generation, va them mechanic theo data.

## Phan can thay doi

### 1. Giam trach nhiem cua `GridManager`

File lien quan:

- `Assets/Scripts/Manager/Gameplay/GridManager.cs`

Van de hien tai:

- File nay dang dai `419` dong.
- Dang vua quan ly registry cho snake, keycard, gate, button, reveal button, wall, portal, deflector, countdown, stop block, arrow shadow, turn-state block, black hole.
- Dong thoi no cung giu event cho key/button va dispatch `IArrowExitListener`.

Rui ro:

- Moi mechanic moi deu bat buoc sua file nay.
- De xay ra merge conflict.
- De tao coupling giua mechanic khong lien quan.
- Sau nay them 5-10 mechanic nua thi `GridManager` se thanh "god object".

Huong doi:

- Tach theo vai tro, khong can tach mot lan:
  - `GridOccupancyRegistry`
  - `GridTriggerRegistry`
  - `GridLinkRegistry` cho portal/deflector
  - `GridBoardEvents` cho key/button/arrow-exit
- Giu `GridManager` la facade cap cao neu can de khong vo code cu ngay lap tuc.

Nen lam som vi:

- Day la diem trung tam anh huong gan nhu moi feature grid.

### 2. Giam su phu thuoc vao singleton va scene lookup

Mau dang lap lai:

- `GridManager.Instance`
- `MessageManager.Instance`
- `FindObjectOfType<...>()`

Vi du ro:

- `SnakeBlock.Start()` tim `LevelController` bang `FindObjectOfType`.
- `LevelLoader` tim `GameCanvas`.
- `LevelController` tim `CameraController`, `WinEffectManager`, `GameCanvas`.
- Nhieu visual va manager tim nhau trong scene luc runtime.

Rui ro:

- Kho test.
- Kho dung lai trong level editor hoac playtest mode.
- Loi ngam xuat hien khi scene setup thay doi.
- Performance lookup tuy chua phai van de lon, nhung maintenance la van de that.

Huong doi:

- Truyen dependency tai luc khoi tao khi co the.
- Dung serialized reference cho cac thanh phan scene co tinh on dinh.
- Tao mot `GameplayContext` hoac `LevelSceneContext` de cap phat cac service chung cho level.

Khong can sua toan bo ngay, nhung bat dau tu:

- `SnakeBlock`
- `LevelLoader`
- `LevelController`

### 3. Cat nho cac class dang qua to

#### `GameCanvas.cs`

File:

- `Assets/Scripts/UI/Screens/GameCanvas.cs`

Tinh trang:

- Khoang `1284` dong.
- Dang vua lo HUD, popup, heart, reward flow, win streak, currency animation, lose flow, tool UI, cinematic intro, transition state.

Rui ro:

- Sua 1 popup de vo popup khac.
- Kho doc, kho onboard nguoi moi.
- Bug UI rat de bi lan sang nhau.

Nen tach thanh:

- `GameHudController`
- `GamePopupController`
- `GameRewardPresentationController`
- `GameHeartController`
- `GameToolPanelController`
- `GameIntroPresentationController`

Khong can tach doi ten hay architecture qua sau ngay.
Chi can dua logic theo cum trach nhiem ra thanh component rieng la da giam rui ro rat nhieu.

#### `LevelLoader.cs`

File:

- `Assets/Scripts/Manager/Level/LevelLoader.cs`

Tinh trang:

- Khoang `425` dong.
- Dang vua clear state, hold transition, spawn obstacle, spawn dots, preload snakes, camera intro, timer setup, tutorial setup.

Rui ro:

- Moi thay doi trong loading flow de gay side effect.
- Kho them che do load moi, load test, load editor, load async.

Nen tach thanh:

- `LevelStateResetter`
- `LevelRuntimeSpawner`
- `LevelLoadingProgress`
- `LevelPostLoadInitializer`

#### `SnakeMover.cs`

File:

- `Assets/Scripts/GamePlay/Snake/SnakeMover.cs`

Tinh trang:

- Khoang `422` dong.
- Da tach khoi `SnakeBlock`, nhung van vua lo blocked movement, exit movement, black hole, stop block, collision, spawn, dash, spin.

Nhan dinh:

- Chua can dap di viet lai.
- Nen giu logic hien tai nhung tiep tuc cat theo nhom hanh vi:
  - `SnakeBlockedMoveFlow`
  - `SnakeExitMoveFlow`
  - `SnakeCollisionResolver`

### 4. Tach ro hon gameplay logic va visual feedback

Tinh trang:

- `SnakeInteractions` hien vua trigger board cell, vua phat SFX/haptic/portal/deflector visual.
- `GridKeycard`, `GridElectricButton` vua la gameplay object, vua chua animation va tween.
- `LevelController` vua lo progression, vua lo camera win, vua lo reward, vua gui message UI.

Rui ro:

- Gameplay bug va visual bug dan vao nhau.
- Kho tat visual de test logic.
- Kho chuyen sang skin/theme/render mode khac.

Huong doi:

- Giu logic grid/rule o layer core.
- Day visual feedback sang presentation component hoac feedback service.
- Gameplay object chi phat event/trang thai, presentation moi quyet dinh tween/VFX/SFX.

Khong can tach qua sach ngay, nhung nen uu tien nhung noi dang dung chung nhieu:

- `SnakeInteractions`
- `LevelController`
- occupant co nhieu tween nhu `GridElectricButton`

### 5. Dinh nghia ro hon mau them mechanic moi

Project da co file:

- `Docs/AddNewMechanicRule.md`

Dinh huong trong file nay la dung, nhung van de la:

- Chua thanh checklist chinh thuc de review PR.
- Chua co test case toi thieu cho mechanic moi.
- File dang gap van de encoding, kho doc trong mot so moi truong.

Nen doi:

- Tao checklist PR cho mechanic moi.
- Chuan hoa lai file docs theo UTF-8.
- Neu duoc, them 1 mau "new mechanic template":
  - dang ky vao grid nhu the nao
  - co anh huong path khong
  - co can guideline update khong
  - co can clear state/reset/reload test khong

## Uu tien refactor de xuat

### Phase 1: Lam ngay, it rui ro, loi ich cao

1. Viet lai va chuan hoa tai lieu ky thuat trong `Docs/`.
2. Bat dau tach `GameCanvas` theo tung component UI nho.
3. Tach bieu dien loading khoi `LevelLoader`.
4. Giam `FindObjectOfType` o `SnakeBlock`, `LevelLoader`, `LevelController`.

Ket qua mong doi:

- Onboard nguoi moi nhanh hon.
- Giam conflict va giam so file "khong ai dam sua".

### Phase 2: Refactor gameplay core vua phai

1. Tach `GridManager` thanh registry/event/facade.
2. Chuyen them query rule qua `BoardState` + `CellOccupancy`.
3. Cat `SnakeMover` thanh flow rieng theo hanh vi.
4. Tach feedback khoi gameplay decisions o `SnakeInteractions`.

Ket qua mong doi:

- Them mechanic moi nhanh hon.
- Logic movement de theo doi va de debug hon.

### Phase 3: Dau tu cho scale lau dai

1. Tao `GameplayContext` hoac `LevelSceneContext` de cap dependency.
2. Them test cho rule layer:
   - scan blocker
   - portal
   - deflector
   - black hole
   - stop block
3. Chuan hoa pattern event thay vi de manager nao cung tu phat event rieng.

Ket qua mong doi:

- Project bot phu thuoc vao scene setup.
- Co kha nang tu tin refactor mechanic ma it vo logic cu.

## Thu tu uu tien file nen dong vao truoc

Neu can lap task cho team, minh de xuat uu tien nhu sau:

1. `Assets/Scripts/UI/Screens/GameCanvas.cs`
2. `Assets/Scripts/Manager/Level/LevelLoader.cs`
3. `Assets/Scripts/Manager/Gameplay/GridManager.cs`
4. `Assets/Scripts/Manager/Level/LevelController.cs`
5. `Assets/Scripts/GamePlay/Snake/SnakeInteractions.cs`
6. `Assets/Scripts/GamePlay/Snake/SnakeMover.cs`

Ly do:

- Day la cac file co tac dong rong, de thanh "nui logic", va gay kho cho maintainability nhieu nhat.

## Thu tu uu tien file nen han che sua vo toi va

Nhung file nay dang la huong dung, nen can giu quy tac mo rong thong qua contract hien co:

1. `Assets/Scripts/GamePlay/Grid/Rules/PathScanner.cs`
2. `Assets/Scripts/GamePlay/Grid/Rules/MoveResult.cs`
3. `Assets/Scripts/GamePlay/Grid/Rules/CellOccupancy.cs`
4. `Assets/Scripts/GamePlay/Grid/Core/GridOccupancyContracts.cs`
5. `Assets/Scripts/GamePlay/Grid/Core/GridOccupantBehaviour.cs`
6. `Assets/Scripts/LevelSystem/Runtime/LevelRuntimeBuilderV2.cs`

Khuyen nghi:

- Chi sua cac file nay khi thay doi contract hoac them mechanic that su can vao rule layer.
- Tranh dua visual-specific logic vao day.

## Checklist review cho moi thay doi sau nay

Khi sua hoac them feature, nen check:

- Logic path co di qua `PathScanner` khong?
- Occupant moi co dung `IGridOccupant`/`IGridTrigger`/`IArrowExitListener` khong?
- Co them direct access moi vao singleton hay `FindObjectOfType` khong?
- Co dua them gameplay logic vao UI/controller khong?
- Co tang them trach nhiem cho `GridManager`, `LevelLoader`, `GameCanvas` khong?
- Feature moi co reset dung khi reload level khong?
- Feature moi co tach duoc giua gameplay va visual feedback khong?

## Ket luan

Project nay khong o trang thai "can viet lai tu dau".

Nen tang hien tai da co nhieu diem dung:

- rule layer cho path
- contract cho occupant
- huong tach `SnakeBlock`
- data-driven runtime factory/builder

Viec nen lam la refactor co kiem soat quanh cac diem nong:

- `GameCanvas`
- `LevelLoader`
- `GridManager`
- cac cho scene lookup/singleton coupling

Neu di theo lo trinh tren, team co the vua tiep tuc them mechanic moi, vua giam no ky thuat dan dan ma khong can dung toan bo development.
