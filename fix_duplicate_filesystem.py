import json

# Read the config file
with open('C:/Users/Mathew Mullen/.claude.json', 'r', encoding='utf-8') as f:
    config = json.load(f)

# Check if there are duplicate filesystem entries
if 'mcpServers' in config:
    # The issue is that there might be two "filesystem" keys in the same object
    # JSON doesn't allow duplicate keys, but during multiple adds they might be in different locations

    # First, let's check what we have
    servers = config.get('mcpServers', {})

    if 'filesystem' in servers:
        current_config = servers['filesystem']
        print("Current filesystem configuration:")
        print(json.dumps(current_config, indent=2))

        # We want the one with D:/GitFolder and D:/Updater
        # Check if it has the right args
        if 'args' in current_config:
            args = current_config['args']
            if 'D:/GitFolder' in args and 'D:/Updater' in args:
                print("\n✓ Filesystem configuration is correct!")
            else:
                print(f"\n✗ Filesystem has wrong directories: {args}")
                print("Expected: D:/GitFolder and D:/Updater")
        else:
            print("\n✗ No args found in filesystem config")

print("\nNote: If there are duplicate entries, they may be in separate configuration objects.")
print("Run 'claude mcp list' to see which configuration is being used.")
