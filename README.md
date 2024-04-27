**BetterBombs** is a [Stardew Valley](https://stardewvalley.net/) mod which increases
the utility of bombs by offering several configurable options to change bomb behavior.

## Contents
* [Install](#install)
* [Configure](#configure)

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
2. Install [an unofficial release of this mod](https://github.com/justastranger/BetterBombs/releases).
3. Run the game using SMAPI.

## Configure
### config.json
The mod creates a `config.json` file in its mod folder the first time you run it. You can open that
file in a text editor to configure the mod.

These are the available settings:

<table>
  <tr>
    <th>Option</th>
    <th>Effect</th>
    <th>Default</th>
    <th>Notes</th>
  </tr>
  <tr>
    <td>
      <code>BreakClumps</code>
    </td>
    <td>
      Master switch for breaking resource clumps - Boulders, Stumps, Logs, etc.
    </td>
    <td>
      <code>False</code>
    </td>
    <td>
      Needs to be enabled in addition to configs for the specific clump types to break.
    </td>
  </tr>
  <tr>
    <td>
      <code>BreakStoneClumps</code>
    </td>
    <td>
      Bombs will break stone resource clumps - Boulders, meteorites, etc.
    </td>
    <td>
      <code>False</code>
    </td>
    <td>
      Requires BreakClumps to be enabled in order to work.
    </td>
  </tr>
  <tr>
    <td>
      <code>BreakWoodClumps</code>
    </td>
    <td>
      Bombs will break wood resource clumps - Stumps, logs, etc.
    </td>
    <td>
      <code>False</code>
    </td>
    <td>
      Requires BreakClumps to be enabled in order to work.
    </td>
  </tr>
  <tr>
    <td>
      <code>BreakWeedsClumps</code>
    </td>
    <td>
      Bombs will break weeds resource clumps that spawn during green rain.
    </td>
    <td>
      <code>False</code>
    </td>
    <td>
    </td>
  </tr>
  <tr>
    <td>
      <code>BreakOtherClumps</code>
    </td>
    <td>
      Bombs will break miscellaneous resource clumps that are added by mods.
    </td>
    <td>
      <code>False</code>
    </td>
    <td>
      Requires BreakClumps to be enabled in order to work. There are no vanilla clumps in this category.
    </td>
  </tr>
  <tr>
    <td>
      <code>CollectMinerals</code>
    </td>
    <td>
      Bombs will collect grabbable minerals on the ground instead of destroying them.
    </td>
    <td>
      <code>False</code>
    </td>
    <td>
    </td>
  </tr>
  <tr>
    <td>
      <code>CollectForage</code>
    </td>
    <td>
      Bombs will collect grabbable forage  on the ground instead of destroying them.
    </td>
    <td>
      <code>False</code>
    </td>
    <td>
      Forage quality respects foraging level and professions.
    </td>
  </tr>
  <tr>
    <td>
      <code>CollectFish</code>
    </td>
    <td>
      Bombs will collect grabbable fish and shells on the ground instead of destroying them.
    </td>
    <td>
      <code>True</code>
    </td>
    <td>
      Forage quality respects foraging level and professions.
    </td>
  </tr>
  <tr>
    <td>
      <code>DamageFarmers</code>
    </td>
    <td>
      Bombs will deal damage to players.
    </td>
    <td>
      <code>False</code>
    </td>
    <td>
    </td>
  </tr>
  <tr>
    <td>
      <code>Radius</code>
    </td>
    <td>
      A multiplier to the default radius of bombs.
    </td>
    <td>
      <code>1.0f</code>
    </td>
    <td>
      [Be careful of values above 3](https://github.com/PhillZitt/BetterBombs/issues/6).
    </td>
  </tr>
  <tr>
    <td>
      <code>Damage</code>
    </td>
    <td>
      A multiplier to the default damage of bombs.
    </td>
    <td>
      <code>1.0f</code>
    </td>
    <td>
    </td>
  </tr>
</table>
