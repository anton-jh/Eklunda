# CLAUDE.md

## What Eklunda is

A small web app for the 2–3 people who share the upkeep of a property. It's built around
**cooperation**: everyone helps out, and the app shows what's been done and who plans to do what.

It runs on a Raspberry Pi on the local network, behind Caddy for HTTPS (see `deploy/`). Users are
trusted: no passwords, you pick your name once and a cookie remembers it. All phones are Android.

## Intentions ("Avsikter")

An intention says **"I plan on taking care of this next, so you don't have to."**

- Offers, never assignments. No turn-taking, no "it's your turn".
- No nagging: no deadlines, no overdue states, no reminders.
- Asking is fine, but "Nej" is always a normal answer.
- Notifications report what happened and may offer the next step ("Jag tar nästa"). Keep them
  light and factual.
- Done is recorded quietly (`DoneAt`, `DoneBy`) and hidden by default.
- You can drop your own intention ("Släpp").
- `DoneWhenEvent` closes an intention automatically when a matching `IEvent` is published. Any new
  heating log entry closes the open "next heating check" intention, whoever made it.
- One open intention per event key. First one wins.

## Rules

- Minimal comments.
- Simple, clear code. No clever code.
- Be consistent.
- No unnecessary duplication.
- No walls of text, long sentences or unnecessary explanations in the UI.
- CRLF line endings (except files under `deploy/`, which run on Linux).
- Point out big issues you find. Fix small ones directly.
- Ask follow-up questions if something is unclear. Do not guess.
- Stay on develop. No new branches if not specifically asked for.

## Conventions

- UI in Swedish, code and comments in English.
- Features live in `Modules/<Name>/`, served without the folder prefix (`/Heating/Log`). Shared
  concepts (users, intentions, events, notifications) live in `Core/`. Register pages in
  `Shared/AppMenu.cs`.
- Modules talk to `Core` through events (`EventService` / `IEventListener`).

## Dev server

- Always stop the dev server (`preview_stop`) when you're done verifying a change. Don't leave it running at the end of a turn.
