# Semantic Model

## Tablice

### Customer
- Id (PK), FirstName, LastName, Email, PhoneNumber, Address
- Veze: 1-N prema Order, 1-N prema Phone

### Phone
- Id (PK), Brand, Model, IMEI, YearOfManufacture, OperatingSystem
- CustomerId (FK -> Customer)
- Veze: 1-N prema RepairJob

### RepairJob
- Id (PK), Description, Status, ReceivedDate, CompletedDate, LaborCost
- PhoneId (FK -> Phone)
- TechnicianId (FK -> Technician)
- Veze: N-N prema SparePart (preko povezane tablice)

### Technician
- Id (PK), FirstName, LastName, Specialization, HireDate, Salary
- Veze: 1-N prema RepairJob

### SparePart
- Id (PK), Name, Manufacturer, Price, StockQuantity
- Veze: N-N prema RepairJob (preko povezane tablice)

### Product
- Id (PK), Name, Description, CurrentPrice, StockQuantity
- Veze: 1-N prema OrderItem

### Order
- Id (PK), OrderDate, TotalAmount, ShippingAddress
- CustomerId (FK -> Customer)
- Veze: 1-N prema OrderItem

### OrderItem
- Id (PK), Quantity, UnitPrice
- ProductId (FK -> Product)
- OrderId (FK -> Order)

## Enumeracije

### RepairStatus
- Pending, InProgress, Completed, Delivered, Cancelled