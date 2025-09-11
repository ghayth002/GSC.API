# GsC API Workflow Diagrams

## Authentication Workflow

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │     │             │
│  Register   │────>│  Verify     │────>│  Login      │────>│  Receive    │
│  Account    │     │  Email      │     │  with       │     │  JWT Token  │
│             │     │             │     │  Credentials│     │             │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                                                                    │
                                                                    ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │     │             │
│  Access     │<────│  Check      │<────│  Check      │<────│  Include    │
│  Protected  │     │  User       │     │  User       │     │  Token in   │
│  Resources  │     │  Permissions│     │  Roles      │     │  Requests   │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
```

## Password Reset Workflow

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │     │             │
│  Request    │────>│  Receive    │────>│  Click      │────>│  Set New    │
│  Password   │     │  Reset      │     │  Reset      │     │  Password   │
│  Reset      │     │  Email      │     │  Link       │     │             │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                                                                    │
                                                                    ▼
                                                            ┌─────────────┐
                                                            │             │
                                                            │  Login with │
                                                            │  New        │
                                                            │  Password   │
                                                            └─────────────┘
```

## User Management Workflow

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │     │             │
│  Admin      │────>│  Create     │────>│  Assign     │────>│  Set User   │
│  Login      │     │  User       │     │  Roles      │     │  Active     │
│             │     │  Account    │     │             │     │  Status     │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                                                                    │
                                                                    ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │     │             │
│  User       │<────│  User       │<────│  User       │<────│  User       │
│  Deletion   │     │  Role       │     │  Profile    │     │  Management │
│  (Optional) │     │  Updates    │     │  Updates    │     │  Dashboard  │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
```

## API Request Processing Workflow

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │     │             │
│  Client     │────>│  Middleware │────>│  Controller │────>│  Service    │
│  Request    │     │  Pipeline   │     │  Action     │     │  Method     │
│             │     │             │     │             │     │             │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                                                                    │
                                                                    ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │     │             │
│  Client     │<────│  Response   │<────│  Controller │<────│  Database   │
│  Response   │     │  Formatting │     │  Processing │     │  Operation  │
│             │     │             │     │             │     │             │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
```

## Email Verification Workflow

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │     │             │
│  Register   │────>│  Generate   │────>│  Send       │────>│  User       │
│  Account    │     │  Verification│     │  Verification│    │  Receives   │
│             │     │  Token      │     │  Email      │     │  Email      │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                                                                    │
                                                                    ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │     │             │
│  User Can   │<────│  Account    │<────│  Verify     │<────│  User Clicks│
│  Login      │     │  Activated  │     │  Token      │     │  Verification│
│             │     │             │     │             │     │  Link       │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
```

## Role and Permission Workflow

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │
│  Define     │────>│  Assign     │────>│  Assign     │
│  Permissions│     │  Permissions│     │  Roles to   │
│             │     │  to Roles   │     │  Users      │
└─────────────┘     └─────────────┘     └─────────────┘
                                               │
                                               ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │
│  Access     │<────│  Check User │<────│  User       │
│  Control    │     │  Permissions│     │  Requests   │
│  Decision   │     │             │     │  Resource   │
└─────────────┘     └─────────────┘     └─────────────┘
```

These workflow diagrams provide a visual representation of the key processes in the GsC API system, helping developers understand how different components interact and how user actions flow through the system.