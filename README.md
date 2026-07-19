# TShop

![Release (latest by date)](https://img.shields.io/github/v/release/TavstalDev/TShop2?style=plastic-square)
![Workflow Status](https://img.shields.io/github/actions/workflow/status/TavstalDev/TShop2/release.yml?branch=stable&label=build&style=plastic-square)
![License](https://img.shields.io/github/license/TavstalDev/TShop2?style=plastic-square)
![Downloads](https://img.shields.io/github/downloads/TavstalDev/TShop2/total?style=plastic-square)
![Issues](https://img.shields.io/github/issues/TavstalDev/TShop2?style=plastic-square)

### What is this?
This is the source code of a .NETFramework library written in C#. This library is a plugin made for Unturned 3.24.x+ servers. 

### Description
A user-friendly item and vehicle shop plugin supporting async (mysql) database. 

### Features
* Customizable item shop
* Customizable vehicle shop
* Custom HUD
* Discount system
* Async SQL Database
* Supports economy plugins that are based on Uconomy

### Requirements
- Unturned 3.24.x or later
- [RocketMod](https://rocketmod.net/) installed on the server
- [Uconomy](https://github.com/Rawrfuls/Uconomy/releases/download/1.2/Uconomy.zip), [Uconomy2](https://github.com/TavstalDev/Uconomy) or other economy plugin
- [Workshop 4.4.0+](https://steamcommunity.com/sharedfiles/filedetails/?id=3763365279)
- [Workshop 4.3.0](https://steamcommunity.com/sharedfiles/filedetails/?id=2767766199)

### Installation

1. Download the latest release and its libraries from the [Releases](https://github.com/TavstalDev/TShop2/releases) page.
2. Place `TShop.dll` into your server's `Rocket/Plugins/` directory.
3. Extract the libraries archive into `Rocket/Libraries` directory.
4. Start or restart the server. The plugin will generate a default YAML configuration file on first load.
5. Edit the configuration file to your liking, then reload the plugin or restart the server.

### Commands
| - means <b>or</b></br>
[] - means <b>required</b></br>
<> - means <b>optional</b>

---
### Player Commands

These commands are for general player use.

| Command Syntax                               | Description                                                      | Permissions                 |
| :------------------------------------------- | :--------------------------------------------------------------- | :-------------------------- |
| `/buy [itemID \| itemName] <amount>`          | Buys a specified `amount` of an item by `itemID` or `itemName`. If `<amount>` is omitted, 1 item is purchased. | `tshop.commands.buy.item`   |
| `/buyvehicle [vehicleID]`                    | Buys the vehicle identified by `vehicleID`.                      | `tshop.commands.buy.vehicle`|
| `/cost [itemID]`                             | Checks the cost of an item using its `itemID`.                   | `tshop.commands.cost.item`  |
| `/costvehicle [vehicleID]`                   | Checks the cost of a vehicle using its `vehicleID`.              | `tshop.commands.cost.vehicle`|
| `/sell [itemID] <amount>`                    | Sells a specified `amount` of an item by `itemID`. If `<amount>` is omitted, all available items are sold. | `tshop.commands.sell.item`  |
| `/sellvehicle`                               | Sells the vehicle you are currently occupying.                   | `tshop.commands.sell.vehicle`|
| `/shop`                                      | Opens the TShop graphical user interface (GUI).                  | `tshop.commands.shopui`     |

### Admin Commands

These commands are for server administrators to manage shop functionalities.

| Command Syntax                                       | Description                                                                                                                               | Permissions                                  |
| :--------------------------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------- |
| `/itemshop add [item name \| id] [buycost] [sellcost] <permission>` | Adds a new item to the shop. Requires `item name` or `id`, `buycost`, `sellcost`, and an optional `permission`.                      | `tshop.commands.itemshop`, `tshop.commands.itemshop.add`     |
| `/itemshop remove [item name \| id]`                  | Removes an item from the shop by `item name` or `id`.                                                                                     | `tshop.commands.itemshop`, `tshop.commands.itemshop.remove`  |
| `/itemshop update [item name \| id] [buycost] [sellcost] <permission>` | Modifies an existing item's `buycost`, `sellcost`, and `permission` in the shop.                                                        | `tshop.commands.itemshop`, `tshop.commands.itemshop.update`  |
| `/vehicleshop add [vehicle name \| id] <buycost> <sellcost> <hexColor> <permission>` | Adds a new vehicle to the shop. Requires `vehicle name` or `id`. Optional `buycost`, `sellcost`, `hexColor` (e.g., `#AABBCC`), and `permission`. | `tshop.commands.vehicleshop`, `tshop.commands.vehicleshop.add` |
| `/vehicleshop remove [vehicle name \| id]`            | Removes a vehicle from the shop by `vehicle name` or `id`.                                                                                | `tshop.commands.vehicleshop`, `tshop.commands.vehicleshop.remove`|
| `/vehicleshop color [vehicle name \| id] [hexColor]`  | Sets or updates the `hexColor` (e.g., `#AABBCC`) of a vehicle in the shop.                                                                  | `tshop.commands.vehicleshop`, `tshop.commands.vehicleshop.color`|
| `/vehicleshop update [vehicle name \| id] <buycost> <sellcost> <permission>` | Modifies an existing vehicle's `buycost`, `sellcost`, and `permission` in the shop.                                                       | `tshop.commands.vehicleshop`, `tshop.commands.vehicleshop.update`|
| `/migratezaupdb [itemtablename] [vehicletablename]`  | Migrates item and vehicle data from a ZaupShop database using the specified `itemtablename` and `vehicletablename`.                     | `tshop.commands.migratezaupdb`               |
| `/removeinvalidproducts`                             | Cleans the database by removing any invalid products (items or vehicles) that no longer exist in Unturned.                                  | `tshop.commands.removeinvalidproducts`       |

---

## Building from Source

### Prerequisites

- .NET Framework 4.8 SDK / targeting pack

### Steps

1. Clone the repository:
   ```
   git clone https://github.com/TavstalDev/TShop.git
   ```
2. Open `TShop.sln` in your IDE.
3. Build the project:
   ```
   dotnet build -c Release
   ```
4. The output DLL will be at `TShop/bin/Release/net48/TShop.dll`.

## License

This project is licensed under the GNU General Public License v3.0. See the `LICENSE` file for more details.

## Contact

For issues or feature requests, please use the [GitHub issue tracker](https://github.com/TavstalDev/TShop2/issues).
