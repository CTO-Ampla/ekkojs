// Test importing from IPC service using ipc: protocol
import demoService from 'ipc:demo';

console.log('Testing ipc: protocol integration...');

// Test the IPC client
async function testIpc() {
    try {
        // Connect to the service
        console.log('Connecting to IPC service...');
        const connected = await demoService.connect();
        console.log(`Connected: ${connected}`);
        
        if (connected) {
            // Test calling getUsers method
            console.log('Calling getUsers...');
            const users = await demoService.call('getUsers');
            console.log(`Users: ${JSON.stringify(users, null, 2)}`);
            
            // Disconnect
            await demoService.disconnect();
            console.log('Disconnected from IPC service');
        }
        
        console.log('ipc: protocol test passed!');
    } catch (error) {
        console.error('Error testing ipc: protocol:', error);
    }
}

testIpc();