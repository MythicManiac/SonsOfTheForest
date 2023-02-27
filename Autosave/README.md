# Autosave

Performs an autosave over a configurable interval, by default once every 5
minutes.

## Motivation

Losing progress due to game crashes or disconnects in multiplayer is no fun.
Autosave should help at least alleviate the issue.

## Functionality

The autosave scheduler starts as soon as the mod gets loaded, and does not reset
at any point. In other words, when the first autosave triggers relative to the
game start will depend on how long was spent in the pre-game lobby.

Naturally, if the autosave schedule triggers in the lobby, nothing will happen.

## Configuration

After launching the game for the first time, a config file will be generated to
`./BepInEx/config/dev.mythic.sotf.autosave.cfg`

The config file contains a single config key called `AutosaveIntervalSeconds`
which can be used to control how many seconds to wait between autosaves.
