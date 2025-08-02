// Test IPC communication with demo service
import ipc from 'ipc:demo';

console.log('🚀 Testing IPC communication...');

async function testIpc() {
    try {
        // Connect to the service
        console.log('📡 Connecting to demo service...');
        const connected = await ipc.connect();
        
        if (!connected) {
            console.error('❌ Failed to connect to demo service');
            return;
        }
        
        console.log('✅ Connected to demo service');
        
        // Get service info
        const serviceInfo = ipc.getServiceInfo();
        if (serviceInfo) {
            console.log('📋 Service Info:', JSON.stringify(serviceInfo, null, 2));
        }
        
        // Test user management
        console.log('\n👤 Testing user management...');
        
        // Create a user
        console.log('Creating user...');
        const createResult = await ipc.call('createuser', {
            name: 'John Doe',
            email: 'john@example.com',
            role: 'user'
        });
        console.log('Create result:', createResult);
        
        // Get all users
        console.log('Getting all users...');
        const users = await ipc.call('getusers');
        console.log('Users:', users);
        
        // Subscribe to user events
        console.log('\n📢 Subscribing to user events...');
        await ipc.subscribe('user-events', async (data) => {
            console.log('🔔 User event received:', data);
        });
        
        // Subscribe to notifications
        console.log('📢 Subscribing to notifications...');
        await ipc.subscribe('notifications', async (data) => {
            console.log('🔔 Notification received:', data);
        });
        
        // Update a user (should trigger events)
        if (users && users.length > 0) {
            const userId = users[0].id;
            console.log(`Updating user ${userId}...`);
            
            const updateResult = await ipc.call('updateuser', {
                id: userId,
                name: 'John Updated',
                email: 'john.updated@example.com'
            });
            console.log('Update result:', updateResult);
        }
        
        // Wait a bit for events
        console.log('⏳ Waiting for events...');
        await new Promise(resolve => setTimeout(resolve, 2000));
        
        console.log('✅ IPC test completed successfully');
        
    } catch (error) {
        console.error('❌ IPC test failed:', error.message);
    } finally {
        // Disconnect
        try {
            await ipc.disconnect();
            console.log('🔌 Disconnected from service');
        } catch (error) {
            console.error('Error disconnecting:', error.message);
        }
    }
}

// Run the test
testIpc().catch(console.error);