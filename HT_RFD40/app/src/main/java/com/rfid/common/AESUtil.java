package com.rfid.common;

import javax.crypto.Cipher;
import javax.crypto.KeyGenerator;
import javax.crypto.SecretKey;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;
import java.security.SecureRandom;
import java.util.Base64;

public class AESUtil {

    private static final String ALGORITHM = "AES";
    private static final String TRANSFORMATION = "AES/CBC/PKCS5Padding";

    // 暗号化
    public static String encrypt(String plainText, String key) throws Exception {
        byte[] keyBytes = getKeyBytes(key);
        SecretKeySpec secretKey = new SecretKeySpec(keyBytes, ALGORITHM);

        // ランダム IV を生成
        byte[] iv = new byte[16];
        SecureRandom random = new SecureRandom();
        random.nextBytes(iv);
        IvParameterSpec ivSpec = new IvParameterSpec(iv);

        Cipher cipher = Cipher.getInstance(TRANSFORMATION);
        cipher.init(Cipher.ENCRYPT_MODE, secretKey, ivSpec);

        byte[] encrypted = cipher.doFinal(plainText.getBytes("UTF-8"));

        // IV + 暗号文を結合して Base64 に変換
        byte[] combined = new byte[iv.length + encrypted.length];
        System.arraycopy(iv, 0, combined, 0, iv.length);
        System.arraycopy(encrypted, 0, combined, iv.length, encrypted.length);

        return Base64.getEncoder().encodeToString(combined);
    }

    // 復号化
    public static String decrypt(String cipherText, String key) throws Exception {
        byte[] combined = Base64.getDecoder().decode(cipherText);
        byte[] iv = new byte[16];
        byte[] encrypted = new byte[combined.length - 16];

        System.arraycopy(combined, 0, iv, 0, 16);
        System.arraycopy(combined, 16, encrypted, 0, encrypted.length);

        byte[] keyBytes = getKeyBytes(key);
        SecretKeySpec secretKey = new SecretKeySpec(keyBytes, ALGORITHM);
        IvParameterSpec ivSpec = new IvParameterSpec(iv);

        Cipher cipher = Cipher.getInstance(TRANSFORMATION);
        cipher.init(Cipher.DECRYPT_MODE, secretKey, ivSpec);

        byte[] decrypted = cipher.doFinal(encrypted);
        return new String(decrypted, "UTF-8");
    }

    // 鍵を16/24/32バイトに調整
    private static byte[] getKeyBytes(String key) throws Exception {
        byte[] keyBytes = new byte[32]; // AES-256
        byte[] parameterKeyBytes = key.getBytes("UTF-8");
        int length = Math.min(parameterKeyBytes.length, keyBytes.length);
        System.arraycopy(parameterKeyBytes, 0, keyBytes, 0, length);
        return keyBytes;
    }

    // 簡易テスト
    public static void main(String[] args) throws Exception {
        String original = "秘密の文字列";
        String key = "MySuperSecretKey123";

        String encrypted = encrypt(original, key);
        System.out.println("暗号化: " + encrypted);

        String decrypted = decrypt(encrypted, key);
        System.out.println("復号化: " + decrypted);
    }
}
