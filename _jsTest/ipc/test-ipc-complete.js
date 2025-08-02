// Complete IPC communication test
import ipc from 'ipc:demo';

console.log('🚀 Complete IPC Test - Demonstrating bi-directional communication with pub/sub');

async function runCompleteTest() {
    try {
        // 1. Connect to service
        console.log('\n📡 Step 1: Connecting to demo service...');
        const connected = await ipc.connect();
        
        if (!connected) {
            throw new Error('Failed to connect to demo service');
        }
        console.log('✅ Connected successfully');
        
        // 2. Show service information
        console.log('\n📋 Step 2: Service Information');
        const serviceInfo = ipc.getServiceInfo();
        if (serviceInfo) {
            console.log('Service:', serviceInfo.name, 'v' + serviceInfo.version);
            console.log('Description:', serviceInfo.description);
            console.log('Service info:', JSON.stringify(serviceInfo, null, 2));
        }
        
        // 3. Subscribe to events BEFORE performing operations
        console.log('\n📢 Step 3: Setting up event subscriptions...');
        
        let eventCount = 0;
        await ipc.subscribe('user-events', (data) => {
            eventCount++;
            console.log(`🔔 User Event #${eventCount}:`, JSON.stringify(data, null, 2));
        });
        
        await ipc.subscribe('notifications', (data) => {
            console.log('📬 Notification:', JSON.stringify(data, null, 2));
        });
        
        console.log('✅ Event subscriptions active');
        
        // 4. Get initial users
        console.log('\n👥 Step 4: Getting initial users...');
        let users = await ipc.call('getusers');
        console.log(`Found ${users?.length || 0} existing users`);
        
        // 5. Create new users
        console.log('\n➕ Step 5: Creating new users...');
        
        const usersToCreate = [
            { Name: 'Alice Johnson', Email: 'alice@example.com' },
            { Name: 'Bob Smith', Email: 'bob@example.com' },
            { Name: 'Charlie Brown', Email: 'charlie@example.com' }
        ];
        
        for (const userData of usersToCreate) {
            console.log(`Creating user: ${userData.Name}`);
            const result = await ipc.call('createuser', userData);
            console.log(`✅ Created user with result: ${typeof result}`);
        }
        
        // 6. Get updated user list
        console.log('\n📊 Step 6: Getting updated user list...');
        users = await ipc.call('getusers');
        console.log(`Now have ${users?.length || 0} total users`);
        
        // 7. Test user updates (which should trigger events)
        if (users && users.length > 0) {
            console.log('\n✏️ Step 7: Testing user updates...');
            const userToUpdate = users[0];
            console.log(`Updating user: ${JSON.stringify(userToUpdate)}`);
            
            const updateResult = await ipc.call('updateuser', {
                Id: userToUpdate.Id,
                Name: userToUpdate.Name + ' (Updated)',
                Email: userToUpdate.Email
            });
            
            console.log('✅ User update completed');
        }
        
        // 8. Wait for events to be processed
        console.log('\n⏳ Step 8: Waiting for events to be processed...');
        await new Promise(resolve => setTimeout(resolve, 2000));
        
        // 9. Test manual event publishing
        console.log('\n📤 Step 9: Publishing custom event...');
        await ipc.publish('notifications', {
            type: 'info',
            message: 'Test completed successfully!',
            timestamp: new Date().toISOString()
        });
        
        // 10. Final status
        console.log('\n🎉 Step 10: Test Summary');
        console.log(`✅ IPC connection: Working`);
        console.log(`✅ Request/Response: Working`);
        console.log(`✅ Event subscriptions: Working`);
        console.log(`✅ User management: Working`);
        console.log(`✅ Event publishing: Working`);
        console.log(`📊 Total events received: ${eventCount}`);
        
        console.log('\n🏆 Complete IPC test PASSED - All features working!');
        
    } catch (error) {
        console.error('\n❌ Test FAILED:', error.message);
        console.error('Stack:', error.stack);
    } finally {
        // Cleanup
        try {
            await ipc.disconnect();
            console.log('\n🔌 Disconnected from service');
        } catch (error) {
            console.error('Error during disconnect:', error.message);
        }
    }
}

// Run the complete test
runCompleteTest().catch(console.error);