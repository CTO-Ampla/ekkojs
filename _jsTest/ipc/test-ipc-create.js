// Test IPC user creation
import ipc from 'ipc:demo';

console.log('🚀 Testing IPC user creation...');

async function test() {
    try {
        console.log('1. Connecting...');
        await ipc.connect();
        
        console.log('2. Creating user...');
        const userData = {
            Name: 'John Doe',
            Email: 'john@example.com'
        };
        
        console.log('User data to send:', JSON.stringify(userData));
        const result = await ipc.call('createuser', userData);
        console.log('Create result:', result);
        console.log('Create result type:', typeof result);
        
        console.log('3. Getting users...');
        const users = await ipc.call('getusers');
        console.log('Users:', users);
        
        await ipc.disconnect();
        console.log('✅ Test done');
        
    } catch (error) {
        console.error('❌ Error:', error.message);
        console.error('Stack:', error.stack);
    }
}

test().catch(console.error);