declare module 'ekko:fs' {
    export function readFile(path: string): string;
    export function readFileSync(path: string): string;
    export function writeFile(path: string, content: string): void;
    export function writeFileSync(path: string, content: string): void;
    export function exists(path: string): boolean;
    export function existsSync(path: string): boolean;
    export function mkdir(path: string): void;
    export function mkdirSync(path: string): void;
    export function readdir(path: string): string[];
    export function readdirSync(path: string): string[];
    export function rm(path: string, options?: { recursive?: boolean }): void;
    export function rmSync(path: string, options?: { recursive?: boolean }): void;
    
    export interface Stats {
        isFile: boolean;
        isDirectory: boolean;
        size: number;
        mtime: Date;
        ctime: Date;
        atime: Date;
    }
    
    export function stat(path: string): Stats;
    export function statSync(path: string): Stats;
    
    const fs: {
        readFile: typeof readFile;
        readFileSync: typeof readFileSync;
        writeFile: typeof writeFile;
        writeFileSync: typeof writeFileSync;
        exists: typeof exists;
        existsSync: typeof existsSync;
        mkdir: typeof mkdir;
        mkdirSync: typeof mkdirSync;
        readdir: typeof readdir;
        readdirSync: typeof readdirSync;
        rm: typeof rm;
        rmSync: typeof rmSync;
        stat: typeof stat;
        statSync: typeof statSync;
    };
    
    export default fs;
}