# TShop

### What is this?
This is a source code of a library written in C#. This library is a plugin made for Unturned 3.24.x+ servers. 

### Description
An user-friendly item and vehicle shop plugin supporting async (mysql) database. 

### Features
* Customizable item shop
* Customizable vehicle shop
* Custom HUD
* Discount system
* Async SQL Database
* Supports economy plugins that are based on Uconomy

### Requirements
- [Uconomy](https://github.com/Rawrfuls/Uconomy/releases/download/1.2/Uconomy.zip) or other economy plugin
- [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=2767766199)

### Commands
| - means <b>or</b></br>
[] - means <b>required</b></br>
<> - means <b>optional</b>

---
### Player Commands
<details>
<summary>/buy [itemID | itemName] <amount></summary>
<b>Description:</b> Buys a specific amount of item(s).
<br>
<b>Permission(s):</b> tshop.commands.buy.item
</details>

<details>
<summary>/buyvehicle [vehicleID]</summary>
<b>Description:</b> Buys a specific vehicle.
<br>
<b>Permission(s):</b> tshop.commands.buy.vehicle
<br>
</details>

<details>
<summary>/cost [itemID]</summary>
<b>Description:</b> Checks the cost of a specific item.
<br>
<b>Permission(s):</b>  tshop.commands.cost.item
</details>

<details>
<summary>/costvehicle [vehicleID] <amount></summary>
<b>Description:</b> Checks the cost of a specific vehicle.
<br>
<b>Permission(s):</b> tshop.commands.cost.vehicle
</details>

<details>
<summary>/sell [itemID] <amount></summary>
<b>Description:</b> Sells a specific amount of item(s).
<br>
<b>Permission(s):</b> tshop.commands.sell.item
</details>

<details>
<summary>/sellvehicle <amount></summary>
<b>Description:</b> Sells the current vehicle.
<br>
<b>Permission(s):</b> tshop.commands.sell.vehicle
</details>

<details>
<summary>/shop</summary>
<b>Description:</b> Opens the UI.
<br>
<b>Permission(s):</b> tshop.commands.shopui
</details>

---
### Admin Commands

<details>
<summary>/itemshop add [item name | id] [buycost] [sellcost] <permission></summary>
<b>Description:</b> Manages the item shop.
<br>
<b>Permission(s):</b>  tshop.commands.itemshop, tshop.commands.itemshop.add
</details>

<details>
<summary>/itemshop remove  [item name | id]</summary>
<b>Description:</b> Manages the item shop.
<br>
<b>Permission(s):</b>  tshop.commands.itemshop, tshop.commands.itemshop.remove
</details>

<details>
<summary>/itemshop update [item name | id] [buycost] [sellcost] <permission></summary>
<b>Description:</b> Manages the item shop.
<br>
<b>Permission(s):</b>  tshop.commands.itemshop, tshop.commands.itemshop.update
</details>

<details>
<summary>/vehicleshop add [vehicle name | id] <buycost> <sellcost> <permission></summary>
<b>Description:</b> Manages the vehicle shop.
<br>
<b>Permission(s):</b>  tshop.commands.vehicleshop, tshop.commands.vehicleshop.add
</details>

<details>
<summary>/vehicleshop remove  [vehicle name | id]</summary>
<b>Description:</b> Manages the vehicle shop.
<br>
<b>Permission(s):</b> tshop.commands.vehicleshop, tshop.commands.vehicleshop.remove
</details>

<details>
<summary>/vehicleshop update [vehicle name | id] <buycost> <sellcost> <permission></summary>
<b>Description:</b> Manages the vehicle shop.
<br>
<b>Permission(s):</b>  tshop.commands.vehicleshop, tshop.commands.vehicleshop.update
</details>

<details>
<summary>/migratezaupdb [itemtablename] [vehicletablename]</summary>
<b>Description:</b> Migrates data from the database of the zaupshop plugin.
<br>
<b>Permission(s):</b>  tshop.commands.migratezaupdb
</details>
